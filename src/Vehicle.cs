using System;
using Microsoft.Xna.Framework;

namespace game;

class Engine {
    // Actually an electric motor but who cares m8
    private float peak_power;
	private float peak_torque;
	private float redline;
	private float base_speed;
	
	public Engine(float peak_power, float peak_torque, float redline) {
		this.peak_power = peak_power * 746.0f; // HP to kW
		this.peak_torque = peak_torque * 1.3558f; // lb-ft to Nm
		this.redline = redline;
		this.base_speed = (this.peak_power / this.peak_torque) * GlobalConsts.INV_PI_DIV_30; 
	}

	public float UpdateTorque(float rpm) {
	    if(rpm <= base_speed) {
			return peak_torque;
		} else if(rpm <= redline) {
			return (peak_power / rpm) * GlobalConsts.INV_PI_DIV_30;
		}
		return 0.0f;
	}
}

class Gearbox {
	private float[] gears;
	private int gear_num;

	public Gearbox(float[] gears) {
		this.gears = gears;
		this.gear_num = 0;
	}

	public void Update(float rpm) {
		if(rpm >= 8500.0f) {
			if(gear_num < gears.Length) {
			    gear_num++;
			}
		} else if(rpm <= 5000.0f) {
			if(gear_num > 0) {
				gear_num--;
			}
		}
	}

	public float GearRatio {
		get {return gears[gear_num];}
	}
}

class VehicleParams {
	public float mass;
	public float inertia;
	public float drag_coef;
	public float cg_to_front;
	public float cg_to_rear;
	public float wheelbase;
	public float wheel_radius;
	public float differential;
	
	public VehicleParams(float mass, float drag_coef, float cg_to_front, float cg_to_rear, float wheel_radius, float differential) {
		this.mass = mass;
		this.inertia = mass;
		this.drag_coef = drag_coef;
		this.cg_to_front = cg_to_front;
		this.cg_to_rear = cg_to_rear;
		this.wheelbase = cg_to_front + cg_to_rear;
		this.wheel_radius = wheel_radius;
		this.differential = differential;
	}
}

class Vehicle {
    private VehicleParams vparams;
	private Engine engine;
	private Gearbox gearbox = new Gearbox(new float[]{3.57f, 2.16f, 1.45f, 1.10f, 0.89f, 0.74f});
	
	public Vector2 velocity_wc;
	
	public int drive_mode;

	public double angle;
	public float angular_velocity;
	private float steering_angle;

	public float rpm;
	public float speed;
	
	public Vehicle(VehicleParams vparams, Engine engine) {
		this.vparams = vparams;
		this.engine = engine;
		this.velocity_wc = Vector2.Zero;
		this.drive_mode = 0;
		this.angle = 0.0;
		this.angular_velocity = 0.0f;
		this.steering_angle = 0.0f;
		this.rpm = 0.0f;
		this.speed = 0.0f;
	}

	public void TurnRight(GameTime gameTime) {
	    if(steering_angle < GlobalConsts.PI/6.0f) {
	        steering_angle += (float)gameTime.ElapsedGameTime.TotalSeconds * GlobalConsts.PI;
		}
	}

	public void TurnLeft(GameTime gameTime) {
		if(steering_angle > -GlobalConsts.PI/6.0f) {
		    steering_angle -= (float)gameTime.ElapsedGameTime.TotalSeconds * GlobalConsts.PI;
		}
	}

	public void ResetSteering(GameTime gameTime) {
		steering_angle -= (float)gameTime.ElapsedGameTime.TotalSeconds * GlobalConsts.TAU * steering_angle;
	}

	public void Update(GameTime gameTime) {
	    gearbox.Update(rpm);
		
	    float sn = (float)Math.Sin(angle);
		float cs = (float)Math.Cos(angle);

		float velocity_x = cs * velocity_wc.Y - sn * velocity_wc.X;
		float velocity_y = sn * velocity_wc.Y + cs * velocity_wc.X;

		float slip_angle_front = (float)Math.Atan2((double)(velocity_y + angular_velocity), (double)Math.Abs(velocity_x)) - Math.Sign(velocity_x) * steering_angle;
		float slip_angle_rear = (float)Math.Atan2((double)(velocity_y - angular_velocity), (double)Math.Abs(velocity_x));

		float weight = vparams.mass * PhysicsConsts.MASS_CONSTANT;

		float lateral_front_y = weight * Math.Clamp(-5.0f * slip_angle_front, -PhysicsConsts.GRIP, PhysicsConsts.GRIP);
		float lateral_rear_y = 0.5f * weight * Math.Clamp(-5.0f * slip_angle_rear, -PhysicsConsts.GRIP, PhysicsConsts.GRIP);
		
		float traction = 0.0f;
		if(drive_mode == 1) {
			traction = engine.UpdateTorque(rpm) * gearbox.GearRatio * vparams.differential / vparams.wheel_radius;
		} else if(drive_mode == -1) {
			traction = -4000.0f;
		}

		float res_x = -(127.0f + vparams.drag_coef * velocity_x * Math.Abs(velocity_x));
		float res_y = -(127.0f + vparams.drag_coef * velocity_y * Math.Abs(velocity_y));

		float force_x = traction + res_x;
		float force_y = (float)Math.Cos((double)steering_angle) * lateral_front_y + lateral_rear_y + res_y;

		speed = velocity_wc.Length();
		if(speed > 10.0f) {
			force_y *= (speed + 1.0f) / 11.0f;
		}

		float angular_torque = vparams.cg_to_front * lateral_front_y - vparams.cg_to_rear * lateral_rear_y;

		float acceleration_x = force_x / vparams.mass;
		float acceleration_y = force_y / vparams.mass;
		
		float acceleration_wc_x = cs * acceleration_y - sn * acceleration_x;
		float acceleration_wc_y = sn * acceleration_y + cs * acceleration_x;
		
	    velocity_wc.X += acceleration_wc_x * 0.02f;
	    velocity_wc.Y += acceleration_wc_y * 0.02f;

		float angular_acceleration = angular_torque / vparams.inertia;

		speed = velocity_wc.Length();
		if(speed < 0.5f && drive_mode != 1) {
		    speed = 0.0f;
			velocity_wc = Vector2.Zero;
			angular_acceleration = 0.0f;
			angular_velocity = 0.0f;
		}

		if(speed < 1.0 && Math.Abs(steering_angle) < 0.05f) {
		    speed = 0.0f;
			angular_velocity = 0.0f;
		} else if (speed < 0.21f) {
			speed = 0.0f;
			angular_velocity = 0.0f;
		}

		rpm = speed / vparams.wheel_radius * gearbox.GearRatio * vparams.differential * GlobalConsts.INV_PI_DIV_30;

		angular_velocity += angular_acceleration * 0.02f;
		angle += angular_velocity * 0.02f;
		angle %= 2.0f * GlobalConsts.PI;
	}
}