using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Shading
{
    public class Camera
    {
        private float nearPlane = 0.001f;
        private float farPlane = 10f;
        private Vector3 pos;
        private Matrix view, projection;
        private float xRot, yRot;
        private Matrix rotation;
        private Vector3 lookAtTarget;
        private float turnSpeed;
        private float moveSpeed;
        private float fieldOfView = 45f;
        private Vector3 dir, up, left;
        protected static Vector3 startUp = Vector3.Up;
        protected static Vector3 startDir = new Vector3(0, 0, -1);

        public Vector3 Dir
        {
            get
            {
                return dir;
            }
        }

        public Matrix View
        {
            get
            {
                return view;
            }
        }

        public Matrix Projection
        {
            get
            {
                return projection;
            }
        }

        /// <summary>
        /// Creates a new first person camera, centered at the origin, looking down the negative Z axis.
        /// </summary>
        /// <param name="turnSpeed">How fast the user can rotate the camera. Value must be greater than 0.1</param>
        /// <param name="moveSpeed">The translational velocity of the camera> Value must be greater than 0</param>
        public Camera(float turnSpeed, float moveSpeed, int screenWidth, int screenHeight)
            : base()
        {
            if (turnSpeed < 0.1f)
            {
                throw new ArgumentOutOfRangeException("turnSpeed", "Turn speed must be greater than 0.1");
            }

            if (moveSpeed <= 0f)
            {
                throw new ArgumentOutOfRangeException("moveSpeed", "Move speed must be greater than 0");
            }

            this.turnSpeed = turnSpeed;
            this.moveSpeed = moveSpeed;

            float aspectRatio = (float)screenWidth / screenHeight;

            pos = Vector3.Zero;
            dir = startDir;
            up = startUp;
            view = Matrix.CreateLookAt(pos, pos + startDir, startUp);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fieldOfView), aspectRatio, nearPlane, farPlane);
        }

        public void Resize(int screenWidth, int screenHeight)
        {
            float aspectRatio = (float)screenWidth / screenHeight;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fieldOfView), aspectRatio, nearPlane, farPlane);
        }

        public void Update(float dt, PlayerIndex index)
        {
            float forwardMovement = 0f;
            float sideMovement = 0f;
            Vector2 rotDelta = Vector2.Zero;
            GamePadState padState = GamePad.GetState(index);

            rotDelta = padState.ThumbSticks.Right;

            xRot -= rotDelta.Y * turnSpeed * dt;
            xRot = MathHelper.Clamp(xRot, -MathHelper.PiOver2, MathHelper.PiOver2);
            yRot -= rotDelta.X * turnSpeed * dt;
            yRot = Math.Sign(yRot) * Math.Abs(yRot % MathHelper.TwoPi);
            rotation = Matrix.CreateRotationX(xRot) * Matrix.CreateRotationY(yRot);
            Vector3.TransformNormal(ref startDir, ref rotation, out dir);
            Vector3.TransformNormal(ref startUp, ref rotation, out up);
            Vector3.Cross(ref up, ref dir, out left);

            forwardMovement += padState.ThumbSticks.Left.Y;
            sideMovement -= padState.ThumbSticks.Left.X;

            pos += dir * forwardMovement * moveSpeed + left * sideMovement * moveSpeed;
            lookAtTarget = pos + dir;

            //Update view with our new info
            Matrix.CreateLookAt(ref pos, ref lookAtTarget, ref up, out view);
        }
    }
}
