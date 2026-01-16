using Godot;

public partial class Jugador : CharacterBody2D
{
    //------Variables-------
    private int _velocidad = 100;
    private string _animacion = "";

    //------Nodos guardados en variables 
    private AnimatedSprite2D _animaciones;
    private RayCast2D _mira;

    public override void _Ready()
    {
        _animaciones = GetNode<AnimatedSprite2D>("sprite");
        _mira = GetNode<RayCast2D>("RayCast2D");
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 movimiento = Vector2.Zero;
        
        if (Input.IsActionPressed("ui_up")) //----arriba
        {
            movimiento.Y -= _velocidad;
            _mira.TargetPosition = new Vector2(0, -50);
            _animacion = "mov_arriba";
        }
        else if (_mira.TargetPosition == new Vector2(0, -50))
        {
            _animacion = "quieto_arriba";
        }
        
        if (Input.IsActionPressed("ui_down")) //-----abajo
        {
            movimiento.Y += _velocidad;
            _mira.TargetPosition = new Vector2(0, 50);
            _animacion = "mov_abajo";
        }
        else if (_mira.TargetPosition == new Vector2(0, 50))
        {
            _animacion = "quieto_abajo";
        }
        
        if (Input.IsActionPressed("ui_left")) //-----izquierda
        {
            movimiento.X -= _velocidad;
            _mira.TargetPosition = new Vector2(-50, 0);
            _animacion = "mov_izquierda";
        }
        else if (_mira.TargetPosition == new Vector2(-50, 0))
        {
            _animacion = "quieto_izquierda";
        }

        if (Input.IsActionPressed("ui_right")) //-----derecha 
        {
            movimiento.X += _velocidad;
            _mira.TargetPosition = new Vector2(50, 0);
            _animacion = "mov_derecha";
        }
        else if (_mira.TargetPosition == new Vector2(50, 0))
        {
            _animacion = "quieto_derecha";
        }

        Velocity = movimiento;
        MoveAndSlide();
        _animaciones.Play(_animacion);
    }
}
