using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace game;

class Player {
	public Vector2 position {get; private set;}
	private Vehicle vehicle;
	private Camera camera;

	public Player(Vehicle vehicle) {
		this.position = new Vector2(100, 100);
		this.vehicle = vehicle;
		this.camera = new Camera(this.position, new Vector2(1280, 720));
	}

	public void Update(GameTime gameTime) {
		if(Keyboard.GetState().IsKeyDown(Keys.Up)) {
		    vehicle.drive_mode = 1;
		} else if(Keyboard.GetState().IsKeyDown(Keys.Down)){
		    vehicle.drive_mode = -1;
		} else {
			vehicle.drive_mode = 0;
		}

		if(Keyboard.GetState().IsKeyDown(Keys.Right)) {
		    vehicle.TurnRight(gameTime);
		} else if(Keyboard.GetState().IsKeyDown(Keys.Left)) {
		    vehicle.TurnLeft(gameTime);
		} else {
			vehicle.ResetSteering(gameTime);
		}
		
		vehicle.Update(gameTime);
		position += vehicle.velocity_wc;
		camera.Update(position, gameTime);
	}

	public int speed {
		get {return (int)(vehicle.velocity_wc.Length() * 3.6f);}
	}

	public int rpm {
		get {return (int)vehicle.rpm;}
	}

	public Matrix CameraMatrix {
		get {return camera.transform;}
	}

	public float heading {
		get {return (float)vehicle.angle;}
	}
}