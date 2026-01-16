using Godot;
using Godot.Collections;
using System.Threading.Tasks;

public partial class Battle : Node2D
{
	public static int battleCounter = -1;
	public static Enemy enemy;

	// Battle state variables
	public bool gonnaAttack = false;
	public bool gonnaAct = false;
	public bool isChoosingAct = false;
	public bool isReadingActText = false;
	public bool isReadingItemText = false;
	public bool isAttacking = false;
	public bool gonnaSpare = false;
	public bool isChoosingItem = false;
	public bool battleWon = false;
	public bool battleLost = false;
	public bool canSpare = false;
	public bool monsterSpeaking = false;
	public int turnCounter = 0;

	public int playerHp
	{
		set
		{
			_playerHp = Mathf.Clamp(value, 0, 20);
			_hpBar.Value = _playerHp;
			_hp2.Text = _playerHp.ToString() + " / 20";
		}
		get { return _playerHp; }
	}
	private int _playerHp = 20;

	public int enemyHp = 500;
	public string enemyName = "Name here";
	public string encounterText = "* name here drew new !";
	public string idleText = "* name here is staring at you angerly";
	public string monsterText = "";
	public int enemyMercy
	{
		set
		{
			_enemyMercy = value;
			if(_enemyMercy >= 100)
			{
				canSpare = true;
			}
		}
		get { return _enemyMercy; }
	}
	private int _enemyMercy = 0;

	public Array<string> acts = new Array<string>();
	[Export] public Array<Item> items = new Array<Item>();
	public Array<PackedScene> bulletWaves = new Array<PackedScene>();

	private PackedScene _button = GD.Load<PackedScene>("uid://ptt71q0lsxgx");

	private TextBox _textBox;
	private MonsterTextBox _monsterTextBox;
	private TextureProgressBar _hpBar;
	private Label _hp2;
	private AudioStreamPlayer _music;
	private Node2D _attackBar;
	private Control _attackLine;
	private CanvasItem _speechBox;
	private Label _damage;
	private Node2D _monster;
	private Sprite2D _monsterSprite;
	private AnimationPlayer _anim;
	private AudioStreamPlayer _selectSound;
	private AudioStreamPlayer _useItemSound;
	private AudioStreamPlayer _soulHitSound;
	private AudioStreamPlayer _monsterHurtSound;
	private AudioStreamPlayer _battleDone;
	private AudioStreamPlayer _shootSound;
	private AudioStreamPlayer _soulBreak;
	private AudioStreamPlayer _knifeSlashSound;
	private Node2D _bullets;
	private AnimatedSprite2D _knife;
	private Control _box;
	private HBoxContainer _buttonsContainer;
	private GridContainer _optionsContainer;
	private Godot.Button _attackButton;
	private Godot.Button _actButton;
	private Godot.Button _itemButton;
	private Godot.Button _mercyButton;
	private Timer _uiCooldownTimer;
	private AudioStreamPlayer _moveSound;

	private int _waveIndex = 0;
	
	// Store signal connections so we can disconnect them
	private Global.WaveDoneEventHandler _waveDoneHandler;
	private Global.AddBulletEventHandler _addBulletHandler;
	private Global.ChangeMercyEventHandler _changeMercyHandler;
	private Global.HealPlayerEventHandler _healPlayerHandler;
	private Global.BulletDestroyedEventHandler _bulletDestroyedHandler;
	private Global.MonsterVisibleEventHandler _monsterVisibleHandler;
	private Global.PlayShootSoundEventHandler _playShootSoundHandler;

	public override async void _Ready()
	{
		// Reset all battle state variables at the start of each battle
		_waveIndex = 0;
		battleWon = false;
		battleLost = false;
		turnCounter = 0;
		gonnaAttack = false;
		gonnaAct = false;
		isChoosingAct = false;
		isReadingActText = false;
		isReadingItemText = false;
		isAttacking = false;
		gonnaSpare = false;
		isChoosingItem = false;
		canSpare = false;
		monsterSpeaking = false;
		
		// Clear currentBossName at the start of battle (will be set by BattleZone if needed)
		// This ensures we don't use a stale boss name from a previous battle
		Global.Singleton.currentBossName = "";
		
		// Get node references
		_textBox = GetNode<TextBox>("%TextBox");
		_monsterTextBox = GetNode<MonsterTextBox>("%MonsterTextBox");
		_hpBar = GetNode<TextureProgressBar>("%HPBar");
		_hp2 = GetNode<Label>("%HP2");
		_music = GetNode<AudioStreamPlayer>("%Music");
		_attackBar = GetNode<Node2D>("%AttackBar");
		_attackLine = GetNode<Control>("%AttackLine");
		_speechBox = GetNode<CanvasItem>("%SpeechBox");
		_damage = GetNode<Label>("Damage");
		_monster = GetNode<Node2D>("%Monster");
		_monsterSprite = GetNode<Sprite2D>("%MonsterSprite");
		_anim = GetNode<AnimationPlayer>("%Anim");
		_selectSound = GetNode<AudioStreamPlayer>("%SelectSound");
		_useItemSound = GetNode<AudioStreamPlayer>("%UseItemSound");
		_soulHitSound = GetNode<AudioStreamPlayer>("%SoulHitSound");
		_monsterHurtSound = GetNode<AudioStreamPlayer>("%MonsterHurtSound");
		_battleDone = GetNode<AudioStreamPlayer>("%BattleDone");
		_shootSound = GetNode<AudioStreamPlayer>("%ShootSound");
		_soulBreak = GetNode<AudioStreamPlayer>("%SoulBreak");
		_knifeSlashSound = GetNode<AudioStreamPlayer>("%KnifeSlashSound");
		_bullets = GetNode<Node2D>("Bullets");
		_knife = GetNode<AnimatedSprite2D>("%Knife");
		_box = GetNode<Control>("%Box");
		_buttonsContainer = GetNode<HBoxContainer>("%ButtonsContainer");
		_optionsContainer = GetNode<GridContainer>("%OptionsContainer");
		_attackButton = GetNode<Godot.Button>("%AttackButton");
		_actButton = GetNode<Godot.Button>("%ActButton");
		_itemButton = GetNode<Godot.Button>("%ItemButton");
		_mercyButton = GetNode<Godot.Button>("%MercyButton");
		_uiCooldownTimer = GetNode<Timer>("%UiCooldownTimer");
		_moveSound = GetNode<AudioStreamPlayer>("%MoveSound");

		// Hide the main UI box initially to prevent flash
		_box.Visible = false;
		
		// Fade from black (match GDScript - called first, not awaited)
		FadeToBlack fade = GetNode<FadeToBlack>("/root/Fade");
		fade.FadeFromBlack();
		
		// Wait a tiny bit for fade to start, then show the box
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		_box.Visible = true;

		// Load songs
		Array<AudioStream> songs = new Array<AudioStream>
		{
			GD.Load<AudioStream>("uid://dpexiickfpwht"),
			GD.Load<AudioStream>("uid://b5k27ym6e01c6")
		};

		GD.Randomize();
		battleCounter += 1;
		_music.Stream = songs[battleCounter % songs.Count];
		_music.Play();

		// Make UI transparent
		Array<CanvasItem> makeMeTransparent = new Array<CanvasItem>
		{
			(CanvasItem)_attackBar,
			(CanvasItem)_attackLine,
			_speechBox,
			_damage
		};
		foreach (CanvasItem ui in makeMeTransparent)
		{
			ui.Show();
			ui.Modulate = new Color(ui.Modulate.R, ui.Modulate.G, ui.Modulate.B, 0.0f);
		}

		// Set up enemy - wait for properties to load
		_monster.AddChild(enemy);
		enemy.AddToGroup("enemy");
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		// Load all properties from scene file - load the scene file directly based on enemy name
		string loadedEncounterText = "";
		Texture2D loadedSprite = null;
		float loadedSpriteScale = 1.0f;
		string enemyNameForPath = enemy.Name; // For error messages
		
		// Try to get the scene file path from the enemy instance's scene file
		string expectedScenePath = "";
		if (enemy.SceneFilePath != null && !string.IsNullOrEmpty(enemy.SceneFilePath))
		{
			expectedScenePath = enemy.SceneFilePath;
			GD.Print($"Battle._Ready: Using enemy's SceneFilePath: '{expectedScenePath}'");
		}
		else
		{
			// Fallback: use enemy name to construct path
			enemyNameForPath = enemy.Name;
			GD.Print($"Battle._Ready: enemy.Name = '{enemy.Name}', enemy.EnemyName = '{enemy.EnemyName}'");
			
			// Try to use EnemyName if available, otherwise fall back to Name
			if (!string.IsNullOrEmpty(enemy.EnemyName) && enemy.EnemyName != "Enemy name here")
			{
				enemyNameForPath = enemy.EnemyName;
				GD.Print($"Using EnemyName for path: '{enemyNameForPath}'");
			}
			
			expectedScenePath = $"res://enemy_data/{enemyNameForPath.ToLower()}.tscn";
			GD.Print($"Battle._Ready: Constructed scene path: '{expectedScenePath}'");
		}
		
		// Load the scene file and instantiate a temporary instance to get properties
		if (ResourceLoader.Exists(expectedScenePath))
		{
			PackedScene sceneFile = GD.Load<PackedScene>(expectedScenePath);
			if (sceneFile != null)
			{
				// Create a temporary instance to read properties
				Node tempInstance = sceneFile.Instantiate();
				if (tempInstance is Enemy tempEnemy)
				{
					// Add to scene tree temporarily to load properties
					AddChild(tempEnemy);
					await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
					await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
					
					// Now the properties should be loaded from the scene file
					// Load sprite
					if (tempEnemy.Sprite != null)
					{
						loadedSprite = tempEnemy.Sprite;
						enemy.Sprite = loadedSprite;
						GD.Print($"Loaded sprite from scene file '{expectedScenePath}': {loadedSprite.ResourcePath}");
					}
					
					// Load sprite scale
					loadedSpriteScale = tempEnemy.SpriteScale;
					enemy.SpriteScale = loadedSpriteScale;
					GD.Print($"Loaded sprite_scale from scene file: {loadedSpriteScale}");
					
					// Load encounter text
					if (!string.IsNullOrEmpty(tempEnemy.EncounterText) && !tempEnemy.EncounterText.Contains("name here"))
					{
						loadedEncounterText = tempEnemy.EncounterText;
						enemy.EncounterText = loadedEncounterText;
						GD.Print($"Loaded encounter_text from scene file '{expectedScenePath}': '{loadedEncounterText}'");
					}
					
					// Load other properties
					if (tempEnemy.Acts != null && tempEnemy.Acts.Count > 0)
					{
						enemy.Acts = tempEnemy.Acts.Duplicate(true);
						GD.Print($"Loaded acts from scene file: {string.Join(", ", enemy.Acts)}");
					}
					else
					{
						GD.PrintErr($"tempEnemy.Acts is null or empty! Count: {(tempEnemy.Acts?.Count ?? 0)}");
					}
					if (tempEnemy.BulletWaves != null && tempEnemy.BulletWaves.Count > 0)
					{
						enemy.BulletWaves = tempEnemy.BulletWaves.Duplicate(true);
						GD.Print($"Loaded bulletWaves from scene file: {enemy.BulletWaves.Count} waves");
					}
					else
					{
						GD.PrintErr($"tempEnemy.BulletWaves is null or empty! Count: {(tempEnemy.BulletWaves?.Count ?? 0)}");
					}
					if (!string.IsNullOrEmpty(tempEnemy.EnemyName) && tempEnemy.EnemyName != "Enemy name here")
					{
						enemy.EnemyName = tempEnemy.EnemyName;
						GD.Print($"Loaded EnemyName from scene file: '{enemy.EnemyName}'");
					}
					
					RemoveChild(tempEnemy);
					tempEnemy.QueueFree();
				}
			}
		}
		else
		{
			GD.PrintErr($"Scene file not found: {expectedScenePath}");
		}

		// Set up enemy (lines 65-74) - match GDScript exactly
		// Use loaded sprite or fallback to enemy.Sprite
		Texture2D spriteToUse = loadedSprite ?? enemy.Sprite;
		_monsterSprite.Texture = spriteToUse;
		if (spriteToUse != null)
		{
			_monsterSprite.Scale *= loadedSpriteScale;
			_monsterSprite.Show();
			_monsterSprite.Modulate = Colors.White;
			GD.Print($"Set monster sprite: {spriteToUse.ResourcePath}, scale: {loadedSpriteScale}");
		}
		else
		{
			GD.PrintErr($"Enemy.Sprite is null! Cannot display boss sprite. Enemy name: {enemyNameForPath}, Scene path: {expectedScenePath}");
		}
		
		_damage.GlobalPosition = MonsterPosition();
		
		// Set battle variables from loaded enemy properties
		enemyName = enemy.Name;
		enemyHp = enemy.HP;
		acts = enemy.Acts != null ? enemy.Acts.Duplicate(true) : new Array<string>();
		bulletWaves = enemy.BulletWaves != null ? enemy.BulletWaves.Duplicate(true) : new Array<PackedScene>();
		
		GD.Print($"Battle variables set - enemyName: '{enemyName}', acts count: {acts.Count}, bulletWaves count: {bulletWaves.Count}");
		
		// Always use loaded encounter text from scene file, never use default
		if (!string.IsNullOrEmpty(loadedEncounterText) && !loadedEncounterText.Contains("name here"))
		{
			encounterText = loadedEncounterText;
			GD.Print($"Battle encounterText set from loaded value: '{encounterText}'");
		}
		else if (!string.IsNullOrEmpty(enemy.EncounterText) && !enemy.EncounterText.Contains("name here"))
		{
			encounterText = enemy.EncounterText;
			GD.Print($"Battle encounterText set from enemy.EncounterText: '{encounterText}'");
		}
		else
		{
			GD.PrintErr($"WARNING: No valid encounter text found! Using fallback.");
			encounterText = "* Enemy appeared!";
		}
		GD.Print($"Final battle encounterText: '{encounterText}'");
		
		await _textBox.Scroll(encounterText);

		// Load items from global inventory
		items = Global.Singleton.battleInventory.Duplicate(true);

		// Connect signals and store handlers for cleanup
		_waveDoneHandler = FinishHell;
		Global.Singleton.WaveDone += _waveDoneHandler;
		
		_addBulletHandler = (Node2D bullet, Transform2D transform) =>
		{
			// Check if battle is still active and objects are valid
			if (battleWon || battleLost)
			{
				// Battle is over, don't add bullets
				if (IsInstanceValid(bullet))
				{
					bullet.QueueFree();
				}
				return;
			}
			
			// Check if _bullets container is valid
			if (!IsInstanceValid(_bullets))
			{
				GD.PrintErr("Battle.AddBullet: _bullets container is not valid!");
				if (IsInstanceValid(bullet))
				{
					bullet.QueueFree();
				}
				return;
			}
			
			// Check if bullet is valid
			if (!IsInstanceValid(bullet))
			{
				GD.PrintErr("Battle.AddBullet: bullet is not valid!");
				return;
			}
			
			// Check if bullet already has a parent (shouldn't happen, but safety check)
			if (bullet.GetParent() != null)
			{
				GD.PrintErr("Battle.AddBullet: bullet already has a parent!");
				return;
			}
			
			_bullets.AddChild(bullet);
			// Set position and rotation separately to preserve scale from scene file
			// Match GDScript: bullet.global_position = position (doesn't overwrite scale)
			// Store scale before setting position to ensure it's preserved
			Vector2 originalScale = bullet.Scale;
			bullet.GlobalPosition = transform.Origin;
			bullet.Rotation = transform.Rotation;
			// Restore scale if it was somehow overwritten (safety check)
			if (bullet.Scale != originalScale)
			{
				bullet.Scale = originalScale;
				GD.Print($"Battle.AddBullet: Restored scale {originalScale} for bullet (was overwritten to {bullet.Scale})");
			}
			// Debug output for jump obstacles to verify scale
			if (bullet is JumpObstacle)
			{
				GD.Print($"Battle.AddBullet: JumpObstacle scale: {bullet.Scale}, position: {bullet.GlobalPosition}");
			}
			// Ensure bullet is visible
			if (bullet is CanvasItem canvasItem)
			{
				canvasItem.Show();
			}
		};
		Global.Singleton.AddBullet += _addBulletHandler;
		
		_changeMercyHandler = (int amount) =>
		{
			enemyMercy += amount;
		};
		Global.Singleton.ChangeMercy += _changeMercyHandler;
		
		_healPlayerHandler = (int amount) =>
		{
			playerHp += amount;
			// Check if battle is still active and audio player is valid
			if (!battleWon && !battleLost && IsInstanceValid(_useItemSound))
			{
				_useItemSound.Play();
			}
		};
		Global.Singleton.HealPlayer += _healPlayerHandler;
		
		_bulletDestroyedHandler = (Vector2 pos) =>
		{
			// Check if battle is still active before creating particles
			if (battleWon || battleLost || !IsInstanceValid(this))
			{
				return;
			}
			PackedScene bulletParticle = GD.Load<PackedScene>("uid://jciddngihwq7");
			Node2D instance = bulletParticle.Instantiate<Node2D>();
			instance.GlobalPosition = pos;
			AddChild(instance);
		};
		Global.Singleton.BulletDestroyed += _bulletDestroyedHandler;
		
		_monsterTextBox.PlayMonsterSpeakAnim += MonsterSpeakingAnim;
		
		_monsterVisibleHandler = (bool newVal) =>
		{
			// Check if battle is still active before creating tween
			if (battleWon || battleLost || !IsInstanceValid(this) || !IsInstanceValid(_monsterSprite))
			{
				return;
			}
			Tween tween = GetTree().CreateTween();
			float finalVal = newVal ? 1.0f : 0.0f;
			tween.TweenProperty(_monsterSprite, "modulate:a", finalVal, 0.5f);
		};
		Global.Singleton.MonsterVisible += _monsterVisibleHandler;
		
		_playShootSoundHandler = () =>
		{
			// Check if battle is still active and audio player is valid
			if (battleWon || battleLost)
			{
				// Battle is over, don't play sounds
				return;
			}
			
			// Check if audio player is valid before playing
			if (IsInstanceValid(_shootSound))
			{
				_shootSound.Play();
			}
		};
		Global.Singleton.PlayShootSound += _playShootSoundHandler;
		RenderingServer.SetDefaultClearColor(Colors.Black);
		_attackButton.GrabFocus();

		// Connect button pressed signals manually (scene file connections may not work in C#)
		_attackButton.Pressed += OnAttackButtonPressed;
		_actButton.Pressed += OnActButtonPressed;
		_itemButton.Pressed += OnItemButtonPressed;
		_mercyButton.Pressed += OnMercyButtonPressed;
		
		// Connect animation signals manually
		_anim.AnimationFinished += (StringName animName) => OnAnimAnimationFinished(animName);
		if (_knife is AnimatedSprite2D knifeSprite)
		{
			knifeSprite.AnimationFinished += OnKnifeAnimationFinished;
		}

		// Connect button focus signals
		foreach (Node child in _buttonsContainer.GetChildren())
		{
			if (child is Godot.Button button)
			{
				button.PivotOffset = button.Size / 2;
				button.FocusEntered += () => OnFocusEntered(button);
			}
		}
	}

	private Vector2 MonsterPosition()
	{
		return (GetTree().GetFirstNodeInGroup("enemy") as Node2D).GlobalPosition;
	}

	public override async void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_accept") && gonnaAttack)
		{
			_selectSound.Play();
			_textBox.Modulate = Colors.White;
			AttackBarVisibility(true);
			await _textBox.ClearText();
			_anim.Play("attack");
			gonnaAttack = false;
			isAttacking = true;
			monsterText = enemy.GetMonsterText();
		}
		else if (@event.IsActionPressed("ui_accept") && isAttacking)
		{
			isAttacking = false;
			_anim.Pause();
			// Stop the attack line from moving so it doesn't trigger the 'Miss' animation.
			Tween tween = GetTree().CreateTween().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Quint);
			tween.TweenProperty(_attackLine, "scale", new Vector2(1.25f, 1.25f), 0.25f);
			tween.TweenProperty(_attackLine, "scale", new Vector2(1, 1), 0.25f);
			_knifeSlashSound.Play();
			_knife.Show();
			_knife.GlobalPosition = MonsterPosition();
			_knife.Play();
		}
		// Rest of the logic is in '_on_knife_animation_finished()'
		else if (@event.IsActionPressed("ui_accept") && gonnaAct)
		{
			_selectSound.Play();
			gonnaAct = false;
			isChoosingAct = true;
			await _textBox.ClearText();
			_textBox.Modulate = Colors.White;
			foreach (string act in acts)
			{
				Godot.Button button = _button.Instantiate<Godot.Button>();
				button.GetNode<RichTextLabel>("text").Text = Util.Shake(act);
				button.FocusExited += () =>
				{
					button.Modulate = new Color(button.Modulate.R, button.Modulate.G, button.Modulate.B, 0.5f);
				};
				button.Pressed += () => DoAct(act);
				_optionsContainer.AddChild(button);
			}
			_optionsContainer.GetChild(0).CallDeferred("grab_focus");
			_uiCooldownTimer.Start();
		}
		// Rest of the logic is in "do_act".
		else if (@event.IsActionPressed("ui_accept") && (isReadingActText || isReadingItemText))
		{
			isReadingActText = false;
			isReadingItemText = false;
			await _textBox.ClearText();
			monsterSpeaking = true;
			monsterText = enemy.GetMonsterText();
			SpeechBubbleVisibility(true);
			_monsterTextBox.Speak(monsterText);
		}
		else if (@event.IsActionPressed("ui_accept") && gonnaSpare)
		{
			if (canSpare)
			{
				gonnaSpare = false;
				canSpare = false;
				_battleDone.Play();
				_music.Stop();
				battleWon = true;
				_monsterSprite.Modulate = new Color(_monsterSprite.Modulate.R, _monsterSprite.Modulate.G, _monsterSprite.Modulate.B, 0.5f);

				CreateTween().TweenProperty(_box, "scale", new Vector2(2, 2), 0.5f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
				CreateTween().TweenProperty(_box, "scale", new Vector2(1, 1), 0.5f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);

				int gold = GD.RandRange(50, 75);
				int exp = 0;

				// Check if this is a boss and award EXP if not already killed
				// Use currentBossName from BattleZone if available, otherwise fall back to enemy.EnemyName
				// This ensures the boss name matches between BattleZone and Battle
				string bossName = !string.IsNullOrEmpty(Global.Singleton.currentBossName) 
					? Global.Singleton.currentBossName 
					: enemy.EnemyName;
				GD.Print($"Battle (Mercy): Checking boss - currentBossName: '{Global.Singleton.currentBossName}', enemy.EnemyName: '{enemy.EnemyName}', using: '{bossName}'");
				if (Global.BOSS_NAMES.Contains(bossName) && !Global.Singleton.IsBossKilled(bossName))
				{
					Global.Singleton.MarkBossKilled(bossName);
					GD.Print($"Battle (Mercy): Marked boss '{bossName}' as killed. IsBossKilled check: {Global.Singleton.IsBossKilled(bossName)}");
					exp = GD.RandRange(25, 50);
					Global.Singleton.AddExp(exp);
					Global.Singleton.playerGold += gold;
				}
				else
				{
					// Regular enemy, still give gold but no EXP
					Global.Singleton.playerGold += gold;
				}

				await _textBox.Scroll($"Battle won\nGot {exp} EXP and {gold} Gold");
				await ToSignal(_textBox, TextBox.SignalName.FinishedScrolling);
				FadeToBlack fade2 = GetNode<FadeToBlack>("/root/Fade");
				await fade2.FadeIntoBlack();

				// Return to last scene, or default to overworld_original if not set
				string returnScene = Global.Singleton.lastScenePath != "" ? Global.Singleton.lastScenePath : "uid://cnxrqinpyif6b";
				GetTree().ChangeSceneToFile(returnScene);
			}
			else if (!canSpare)
			{
				_selectSound.Play();
			}
		}
		else if (@event.IsActionPressed("ui_accept") && monsterSpeaking)
		{
			monsterSpeaking = false;
			_monsterTextBox.StopTalking();
			SpeechBubbleVisibility(false);
			StartHell();
		}
		else if (@event.IsActionPressed("ui_cancel"))
		{
			if (gonnaAttack)
			{
				gonnaAttack = false;
				_buttonsContainer.Show();
				_attackButton.GrabFocus();
				_textBox.Modulate = Colors.White;
				_textBox.Scroll(turnCounter > 0 ? idleText : encounterText);
			}
			else if (gonnaAct)
			{
				_actButton.GrabFocus();
				_buttonsContainer.Show();
				_optionsContainer.Hide();
				_textBox.Modulate = Colors.White;
				_textBox.Scroll(turnCounter > 0 ? idleText : encounterText);
				gonnaAct = false;
			}
			else if (isChoosingAct)
			{
				foreach (Node child in _optionsContainer.GetChildren())
				{
					child.QueueFree();
				}
				_optionsContainer.Hide();
				if (canSpare)
				{
					_textBox.Modulate = Colors.Yellow;
				}
				_textBox.SetNewText("* " + enemyName);
				isChoosingAct = false;
				gonnaAct = true;
			}
			else if (isChoosingItem)
			{
				foreach (Node child in _optionsContainer.GetChildren())
				{
					child.QueueFree();
				}
				_optionsContainer.Hide();
				_buttonsContainer.Show();
				_itemButton.GrabFocus();
				_textBox.Modulate = Colors.White;
				_textBox.Scroll(turnCounter > 0 ? idleText : encounterText);
				isChoosingItem = false;
			}
			else if (gonnaSpare)
			{
				_mercyButton.GrabFocus();
				_buttonsContainer.Show();
				_optionsContainer.Hide();
				_textBox.Modulate = Colors.White;
				_textBox.Scroll(turnCounter > 0 ? idleText : encounterText);
				gonnaSpare = false;
			}
		}
	}

	public void PlayerTakeDamage(int amount, Soul soul)
	{
		playerHp -= amount;
		_soulHitSound.Play();

		if (playerHp <= 0)
		{
			_music.Stop();
			_soulBreak.Play();
			battleLost = true;
			Node2D attack = GetTree().GetFirstNodeInGroup("wave") as Node2D;
			if (IsInstanceValid(attack))
			{
				attack.QueueFree();
			}
			PackedScene DEATH_PARTICLE = GD.Load<PackedScene>("uid://cvsoixker4k6d");
			Node2D particles = DEATH_PARTICLE.Instantiate<Node2D>();
			if (particles is CpuParticles2D particles2D)
			{
				// Set the color - CPUParticles2D uses Color property to tint particles
				particles2D.Color = soul.Color;
				// Also set the modulate to ensure color is applied
				particles2D.Modulate = soul.Color;
				GD.Print($"Death particle color set to: {soul.Color}, Soul mode: {soul.SoulMode}");
			}
			particles.GlobalPosition = soul.GlobalPosition;
			AddChild(particles);
			soul.CallDeferred("queue_free");
			ChangeBoxSize(new Vector2(1.0f, 1.0f));
			if (particles is CpuParticles2D particles2D2)
			{
				particles2D2.Finished += async () =>
				{
					_textBox.Scroll("Battle Lost...");
					await ToSignal(_textBox, TextBox.SignalName.FinishedScrolling);
					FadeToBlack fade = GetNode<FadeToBlack>("/root/Fade");
					await fade.FadeIntoBlack();

					// Return to last scene, or default to overworld_original if not set
					string returnScene = Global.Singleton.lastScenePath != "" ? Global.Singleton.lastScenePath : "uid://cnxrqinpyif6b";
					GetTree().ChangeSceneToFile(returnScene);
				};
			}
		}
	}

	private void OnAttackButtonPressed()
	{
		_selectSound.Play();
		_buttonsContainer.Hide();
		if (canSpare)
		{
			_textBox.Modulate = Colors.Yellow;
		}
		_textBox.SetNewText("* " + enemyName);
		gonnaAttack = true;
	}

	private async void OnAnimAnimationFinished(StringName animName)
	{
		if (animName == "attack")
		{
			isAttacking = false;
			_attackLine.Modulate = new Color(_attackLine.Modulate.R, _attackLine.Modulate.G, _attackLine.Modulate.B, 0.0f);
			_damage.Text = "miss";
			await DamageLabelBounce();
			OnAnimAnimationFinished("monster_hurt");
		}
		else if (animName == "die")
		{
			await ChangeBoxSize(new Vector2(1.0f, 1.0f));
			_textBox.Modulate = Colors.Red;
			int exp = GD.RandRange(25, 50);
			int gold = GD.RandRange(20, 30);

			// Check if this is a boss and award EXP if not already killed
			// Use currentBossName from BattleZone if available, otherwise fall back to enemy.EnemyName
			// This ensures the boss name matches between BattleZone and Battle
			string bossName = !string.IsNullOrEmpty(Global.Singleton.currentBossName) 
				? Global.Singleton.currentBossName 
				: enemy.EnemyName;
			GD.Print($"Battle (Attack): Checking boss - currentBossName: '{Global.Singleton.currentBossName}', enemy.EnemyName: '{enemy.EnemyName}', using: '{bossName}'");
			if (Global.BOSS_NAMES.Contains(bossName) && !Global.Singleton.IsBossKilled(bossName))
			{
				Global.Singleton.MarkBossKilled(bossName);
				GD.Print($"Battle (Attack): Marked boss '{bossName}' as killed. IsBossKilled check: {Global.Singleton.IsBossKilled(bossName)}");
				Global.Singleton.AddExp(exp);
				Global.Singleton.playerGold += gold;
			}
			else
			{
				// Regular enemy, still give gold but no EXP
				Global.Singleton.playerGold += gold;
				exp = 0;
			}

			await _textBox.Scroll($"Battle won\nGot {exp} EXP and {gold} Gold");
			await ToSignal(_textBox, TextBox.SignalName.FinishedScrolling);
			FadeToBlack fade = GetNode<FadeToBlack>("/root/Fade");
			await fade.FadeIntoBlack();

			// Return to last scene, or default to overworld_original if not set
			string returnScene = Global.Singleton.lastScenePath != "" ? Global.Singleton.lastScenePath : "uid://cnxrqinpyif6b";
			GetTree().ChangeSceneToFile(returnScene);
		}
		else if (animName == "monster_hurt")
		{
			AttackBarVisibility(false);
			if (enemyHp <= 0)
			{
				battleWon = true;
				_anim.Play("die");
				_music.Stop();
				_battleDone.Play();
				return;
			}
			// The rest of the logic happens in the function "_on_anim_animation_finished()"...
			monsterSpeaking = true;
			SpeechBubbleVisibility(true);
			_monsterTextBox.Speak(monsterText);
		}
	}

	private async void StartHell()
	{
		_buttonsContainer.Hide();
		
		if (bulletWaves == null || bulletWaves.Count == 0)
		{
			GD.PrintErr($"ERROR: bulletWaves is null or empty! Cannot start wave. Count: {(bulletWaves?.Count ?? 0)}");
			return;
		}
		
		GD.Print($"Starting wave {_waveIndex} from {bulletWaves.Count} available waves");
		Node2D wave = bulletWaves[_waveIndex % bulletWaves.Count].Instantiate<Node2D>();
		_waveIndex += 1;

		// Add wave to scene tree FIRST to ensure properties are loaded
		AddChild(wave);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		// Get wave.mode AFTER adding to scene tree (properties are now loaded)
		Soul.Mode soulMode = Soul.Mode.Red;
		if (wave is Wave waveInstance)
		{
			soulMode = waveInstance.Mode;
			GD.Print($"Got soul mode from Wave instance: {soulMode}");
		}
		else
		{
			// Try to get mode from the wave instance
			var modeVariant = wave.Get("mode");
			if (modeVariant.VariantType == Variant.Type.Int)
			{
				soulMode = (Soul.Mode)modeVariant.AsInt32();
				GD.Print($"Got soul mode from Get('mode'): {soulMode}");
			}
			else
			{
				// Try snake_case
				var modeVariant2 = wave.Get("Mode");
				if (modeVariant2.VariantType == Variant.Type.Int)
				{
					soulMode = (Soul.Mode)modeVariant2.AsInt32();
					GD.Print($"Got soul mode from Get('Mode'): {soulMode}");
				}
			}
		}

		// Create soul AFTER wave is added (so FollowSoul can find it, but mode is loaded)
		Soul soul = Soul.NewSoul(soulMode);
		soul.GlobalPosition = _attackBar.GlobalPosition;
		soul.TookDamage += PlayerTakeDamage;
		AddChild(soul); // Add soul to scene tree

		// Get box_size and box_size_change_time - match GDScript: wave.box_size, wave.box_size_change_time
		Vector2 boxSize = new Vector2(1.0f, 1.0f);
		float boxSizeChangeTime = 0.3f;
		
		// Try C# property first (like GDScript does directly)
		if (wave is Wave waveInstance2)
		{
			boxSize = waveInstance2.BoxSize;
			boxSizeChangeTime = waveInstance2.BoxSizeChangeTime;
			GD.Print($"Got box_size from Wave instance: {boxSize}, boxSizeChangeTime: {boxSizeChangeTime}");
		}
		
		// Also try loading from scene file using Get() as fallback or to verify
		var boxSizeVariant = wave.Get("box_size");
		if (boxSizeVariant.VariantType == Variant.Type.Vector2)
		{
			Vector2 loadedBoxSize = boxSizeVariant.AsVector2();
			if (boxSize == new Vector2(1.0f, 1.0f) || loadedBoxSize != new Vector2(1.0f, 1.0f))
			{
				boxSize = loadedBoxSize;
				GD.Print($"Loaded box_size from Get(): {boxSize}");
			}
		}
		var boxSizeChangeTimeVariant = wave.Get("box_size_change_time");
		if (boxSizeChangeTimeVariant.VariantType == Variant.Type.Float)
		{
			float loadedTime = boxSizeChangeTimeVariant.AsSingle();
			if (boxSizeChangeTime == 0.3f || loadedTime != 0.3f)
			{
				boxSizeChangeTime = loadedTime;
				GD.Print($"Loaded box_size_change_time from Get(): {boxSizeChangeTime}");
			}
		}

		GD.Print($"Starting hell - changing box size to: {boxSize}, time: {boxSizeChangeTime}");
		GD.Print($"Current box scale before change: {_box.Scale}");
		await ChangeBoxSize(boxSize, boxSizeChangeTime);
		GD.Print($"Box scale after change: {_box.Scale}");
		// soul and wave are already added above
	}

	// The wave finishes when the Node emits the global "wave_done" signal.
	private async void FinishHell(Node2D wave, Soul soul)
	{
		// Extract properties from wave BEFORE freeing it (match GDScript: access wave.box_size_change_time before queue_free)
		float boxSizeChangeTime = 0.3f;
		if (IsInstanceValid(wave))
		{
			if (wave is Wave waveInstance)
			{
				boxSizeChangeTime = waveInstance.BoxSizeChangeTime;
			}
			else
			{
				var boxSizeChangeTimeVariant = wave.Get("box_size_change_time");
				if (boxSizeChangeTimeVariant.VariantType == Variant.Type.Float)
				{
					boxSizeChangeTime = boxSizeChangeTimeVariant.AsSingle();
				}
			}
		}
		
		// Now free the wave
		if (IsInstanceValid(wave))
		{
			wave.QueueFree();
		}
		
		// Clear all bullets when wave finishes
		if (IsInstanceValid(_bullets))
		{
			// Get children list before iterating to avoid issues with disposed objects
			Godot.Collections.Array<Node> bulletChildren = _bullets.GetChildren();
			foreach (Node child in bulletChildren)
			{
				if (IsInstanceValid(child))
				{
					child.QueueFree();
				}
			}
		}
		
		if (battleWon || battleLost) return;
		turnCounter += 1;
		_buttonsContainer.Show();
		
		if (IsInstanceValid(soul))
		{
			soul.QueueFree();
		}

		await ChangeBoxSize(new Vector2(1.0f, 1.0f), boxSizeChangeTime);
		idleText = enemy.GetIdleText();
		await _textBox.Scroll(idleText);
		_attackButton.GrabFocus();
	}

	private void OnActButtonPressed()
	{
		_selectSound.Play();
		_buttonsContainer.Hide();
		_optionsContainer.Show();
		if (canSpare)
		{
			_textBox.Modulate = Colors.Yellow;
		}
		_textBox.SetNewText("* " + enemyName);
		gonnaAct = true;
	}

	public void DoAct(string actName)
	{
		if (_uiCooldownTimer.TimeLeft > 0)
		{
			return;
		}
		_selectSound.Play();
		foreach (Node child in _optionsContainer.GetChildren())
		{
			child.QueueFree();
		}
		_optionsContainer.Hide();
		_textBox.Scroll(enemy.DoActGetText(actName));
		isChoosingAct = false;
		isReadingActText = true;
	}

	public void UseItem(Item item)
	{
		if (_uiCooldownTimer.TimeLeft > 0)
		{
			return;
		}
		_useItemSound.Play();
		foreach (Node child in _optionsContainer.GetChildren())
		{
			child.QueueFree();
		}
		_optionsContainer.Hide();
		playerHp += item.Amount;
		_textBox.Scroll(item.Text);
		items.Remove(item);

		// Also remove from global inventory by finding matching item
		for (int i = 0; i < Global.Singleton.battleInventory.Count; i++)
		{
			if (Global.Singleton.battleInventory[i].ItemName == item.ItemName)
			{
				Global.Singleton.battleInventory.RemoveAt(i);
				break;
			}
		}
		isChoosingItem = false;
		isReadingItemText = true;
	}

	private void OnMercyButtonPressed()
	{
		_selectSound.Play();
		gonnaSpare = true;
		_buttonsContainer.Hide();
		if (canSpare)
		{
			_textBox.Modulate = Colors.Yellow;
		}
		_textBox.SetNewText("* " + enemyName);
	}

	private void OnItemButtonPressed()
	{
		_selectSound.Play();
		if (items.Count <= 0)
		{
			return;
		}
		isChoosingItem = true;
		_buttonsContainer.Hide();
		_optionsContainer.Show();
		_textBox.ClearText();
		foreach (Item item in items)
		{
			Godot.Button button = _button.Instantiate<Godot.Button>();
			button.GetNode<RichTextLabel>("text").Text = Util.Shake(item.ItemName);
			button.FocusExited += () =>
			{
				button.Modulate = new Color(button.Modulate.R, button.Modulate.G, button.Modulate.B, 0.5f);
			};
			button.Pressed += () => UseItem(item);
			_optionsContainer.AddChild(button);
		}
		_optionsContainer.GetChild(0).CallDeferred("grab_focus");
		_uiCooldownTimer.Start();
	}

	// Rest of the logic is in "use_item".
	public void AddChoicesButtons()
	{
		// Empty for now
	}

	private void OnKnifeAnimationFinished()
	{
		_knife.Hide();
		_monsterHurtSound.Play();
		float distanceFromCentre = Mathf.Round(Mathf.Abs(_attackLine.GlobalPosition.X - _attackBar.GlobalPosition.X));
		int damage = Mathf.RoundToInt((575 - distanceFromCentre) / 10);
		_damage.Text = damage.ToString();
		DamageLabelBounce();
		enemyHp -= damage;
		_anim.Play("monster_hurt");
	}

	// The rest of the logic happens in the function "_on_anim_animation_finished()"...
	private void MonsterSpeakingAnim()
	{
		// Animation:-
		Tween tween = GetTree().CreateTween();
		Vector2 ogDim = _monsterSprite.Scale;
		Vector2 animDim = ogDim + new Vector2(ogDim.X * 0.1f, ogDim.Y * -0.05f);
		float delta = 0.2f;
		for (int i = 0; i < 2; i++)
		{
			tween.TweenProperty(_monsterSprite, "scale", animDim, delta);
			tween.TweenProperty(_monsterSprite, "scale", ogDim, delta);
		}
	}

	private async Task ChangeBoxSize(Vector2 newSize, float delta = 0.3f)
	{
		Tween tween = GetTree().CreateTween().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Cubic);
		tween.TweenProperty(_box, "scale:x", newSize.X, delta);
		tween.TweenProperty(_box, "scale:y", newSize.Y, delta);
		await ToSignal(tween, Tween.SignalName.Finished);
	}

	public override void _Process(double delta)
	{
		QueueRedraw();
	}
	
	public override void _ExitTree()
	{
		// Disconnect all Global signals to prevent accessing disposed objects
		// This is critical in C# - GDScript does this automatically, but C# doesn't
		if (Global.Singleton != null)
		{
			if (_waveDoneHandler != null)
			{
				Global.Singleton.WaveDone -= _waveDoneHandler;
			}
			if (_addBulletHandler != null)
			{
				Global.Singleton.AddBullet -= _addBulletHandler;
			}
			if (_changeMercyHandler != null)
			{
				Global.Singleton.ChangeMercy -= _changeMercyHandler;
			}
			if (_healPlayerHandler != null)
			{
				Global.Singleton.HealPlayer -= _healPlayerHandler;
			}
			if (_bulletDestroyedHandler != null)
			{
				Global.Singleton.BulletDestroyed -= _bulletDestroyedHandler;
			}
			if (_monsterVisibleHandler != null)
			{
				Global.Singleton.MonsterVisible -= _monsterVisibleHandler;
			}
			if (_playShootSoundHandler != null)
			{
				Global.Singleton.PlayShootSound -= _playShootSoundHandler;
			}
		}
		
		// Also disconnect local signal
		if (_monsterTextBox != null && IsInstanceValid(_monsterTextBox))
		{
			_monsterTextBox.PlayMonsterSpeakAnim -= MonsterSpeakingAnim;
		}
		
		base._ExitTree();
	}

	public override void _Draw()
	{
		Rect2 boxRect = new Rect2(_box.GlobalPosition, _box.Size * _box.Scale);
		DrawRect(boxRect, Colors.White, false, 10);
	}

	private void AttackBarVisibility(bool visible_)
	{
		float newVal = visible_ ? 1.0f : 0.0f;
		float delta = visible_ ? 0.05f : 0.2f;
		Tween tween = GetTree().CreateTween();
		tween.SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Bounce).SetParallel();
		tween.TweenProperty(_attackLine, "modulate:a", newVal, delta);
		tween.TweenProperty(_attackBar, "modulate:a", newVal, delta);
	}

	private void SpeechBubbleVisibility(bool visible_)
	{
		float newVal = visible_ ? 1.0f : 0.0f;
		Tween tween = GetTree().CreateTween();
		tween.SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Bounce).SetParallel();
		tween.TweenProperty(_speechBox, "modulate:a", newVal, 0.25f);
	}

	private void OnFocusEntered(Godot.Button button)
	{
		button.Modulate = new Color(button.Modulate.R, button.Modulate.G, button.Modulate.B, 1.0f);
		_moveSound.Play();
		Tween tween = GetTree().CreateTween().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Bounce);
		tween.TweenProperty(button, "scale", new Vector2(1.5f, 1.5f), 0.2f);
		tween.TweenProperty(button, "scale", new Vector2(1, 1), 0.1f);
	}

	private async Task DamageLabelBounce()
	{
		Tween tween = CreateTween().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Cubic);
		tween.TweenProperty(_damage, "modulate:a", 1.0f, 0.1f);
		tween.TweenProperty(_damage, "scale", new Vector2(1.5f, 1.5f), 0.2f);
		tween.TweenProperty(_damage, "scale", new Vector2(1, 1), 0.1f);
		tween.TweenProperty(_damage, "modulate:a", 0.0f, 0.1f);
		await ToSignal(tween, Tween.SignalName.Finished);
	}
}
