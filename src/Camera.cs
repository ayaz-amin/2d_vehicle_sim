using System;
using Microsoft.Xna.Framework;

namespace game;

class Camera {
	private Vector2 position;
	private Vector2 window_size;
	private double t;
	public Matrix transform {get; private set;}
		
	public Camera(Vector2 position, Vector2 window_size) {
		this.position = position;
		this.window_size = new Vector2(
			window_size.X / 2,
			window_size.Y / 2
		);
	}

	public void Update(Vector2 new_pos, GameTime gameTime) {
		t = 1.0 - Math.Exp(-20.0 * gameTime.ElapsedGameTime.TotalSeconds);
		position = Vector2.Lerp(position, new_pos, (float)t);
		Matrix offset = Matrix.CreateTranslation(window_size.X, window_size.Y, 0);
		transform = Matrix.CreateTranslation(-position.X, -position.Y, 0) * offset;
	}
}