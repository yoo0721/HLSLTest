using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D11;

namespace PhysicsTestOnSlimDX
{
    public class Camera
    {
        Vector3 eye;
        Vector3 target;
        Vector3 up;

        public float Speed;

        public Camera()
        {
            eye = new Vector3(0, 0, 0);
            target = new Vector3(1,1,1);
            up = new Vector3(0, 1, 0);
        }
        public Matrix ViewMatrix
        {
            get
            {
                return Matrix.LookAtRH(eye, target, up);
            }
        }

        public string TEXT
        {
            get { return eye.ToString() + target.ToString(); }
        }


        public void MoveLR(float amount)
        {
            Vector3 direction = Vector3.Subtract(target, eye);
            direction = new Vector3(direction.Z, 0, -1.0f * direction.X);
            direction.Normalize();
            direction = Vector3.Multiply(direction, amount);
            eye = Vector3.Add(eye, direction);
            target = Vector3.Add(target, direction);
        }

        // 前進・後退
        public void MoveFB(float amount)
        {
            Vector3 direction = Vector3.Subtract(target, eye);
            direction.Normalize();
            direction = Vector3.Multiply(direction, amount);
            eye = Vector3.Add(eye, direction);
            target = Vector3.Add(target, direction);
        }

        // 上下移動
        public void MoveUD(float amount)
        {
            var direction = new Vector3(up.X, up.Y, up.Z);
            direction.Normalize();
            direction = Vector3.Multiply(direction, amount);
            eye = Vector3.Add(eye, direction);
            target = Vector3.Add(target, direction);
        }

        public void RotationAxis(float amount)
        {
            Vector3 v1 = Vector3.Subtract(target, eye);
            Vector3 v2 = new Vector3(v1.Z, 0, -1.0f * v1.X);
            Vector3 v3 = Vector3.Cross(v1, v2);
            Matrix rot = Matrix.RotationAxis(v3, amount);
            Vector4 direction4 = Vector3.Transform(v1, rot);
            Vector3 direction3 = new Vector3(direction4.X, direction4.Y, direction4.Z);
            direction3.Normalize();
            direction3 = Vector3.Multiply(direction3, v1.Length());

            target = Vector3.Add(eye, direction3);
        }

        public void RotationZX(float amount)
        {
            Vector3 v1 = Vector3.Subtract(target, eye);
            Vector3 v2 = new Vector3(v1.Z, 0, -v1.X);
            Matrix rot = Matrix.RotationAxis(v2, amount);
            Vector4 direction4 = Vector3.Transform(v1, rot);
            Vector3 direction3 = new Vector3(direction4.X, direction4.Y, direction4.Z);
            direction3.Normalize();
            direction3 = Vector3.Multiply(direction3, v1.Length());

            target = Vector3.Add(eye, direction3);
        }
    }
}
