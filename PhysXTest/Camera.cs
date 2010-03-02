using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using StillDesign.PhysX;

namespace PhysXTest
{
    /**
     * This is a simple and fairly inefficient camera class taken from the camera class in the PhysX.Net samples.  It
     * could be improved so that it stores more information and therefore has to do less calculation each update cycle,
     * but that shouldn't be worried about unless it is deemed to be an issue (by way of profiling).
     */
    class Camera
    {

        #region Variables

        private Game _game;
        private float _pitch, _yaw;

        #endregion

        internal Camera(Game game)
        {
            _game = game;

            this.View = Matrix.CreateLookAt(new Vector3(0, 20, 90), new Vector3(0, 20, 0), Vector3.UnitY);
            this.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                  _game.GraphicsDevice.Viewport.AspectRatio,
                                                                  0.1f, 10000.0f);

            CenterCursor();
        }

        public void Update(GameTime elapsedTime)
        {
            GameWindow window = _game.Window;

            // Get the mouse's offset from the previous position (window center).
            Vector2 mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            Vector2 mouseCenter = new Vector2(window.ClientBounds.Width / 2, window.ClientBounds.Height / 2);
            Vector2 mouseDelta = mousePosition - mouseCenter;
            Vector2 deltaDampened = mouseDelta * 0.0005f;
            CenterCursor();

            // Modify the yaw and pitch to take the mouse movement into account.
            _yaw -= deltaDampened.X;
            _pitch -= deltaDampened.Y;

            // Get the camera position from the view matrix.
            // This is something that can be stored instead of recalculated.  Getting matrix inverses is _slow_.
            Vector3 position = Matrix.Invert(this.View).Translation;

            // Get the new rotation matrix from the new yaw and pitch values.
            Matrix cameraRotation = Matrix.CreateFromYawPitchRoll(_yaw, _pitch, 0.0f);

            // Now we can get the new look vector.
            Vector3 newForward = Vector3.TransformNormal(Vector3.Forward, cameraRotation);

            float elapsed = (float) elapsedTime.ElapsedGameTime.TotalSeconds;
            const float speed = 20.0f; // Movement is 20 units per second.
            float distance = speed * elapsed;  // dx = v*t

            // The amount to shift the position by starts at 0.
            Vector3 translateDirection = Vector3.Zero;

            // We get the current state of the keyboard...
            KeyboardState states = Keyboard.GetState();

            // And use that to determine which directions to move in.
            if (states.IsKeyDown(Keys.W)) // Forwards?
                translateDirection += Vector3.TransformNormal(Vector3.Forward, cameraRotation);

            if (states.IsKeyDown(Keys.S)) // Backwards?
                translateDirection += Vector3.TransformNormal(Vector3.Backward, cameraRotation);

            if (states.IsKeyDown(Keys.A)) // Left?
                translateDirection += Vector3.TransformNormal(Vector3.Left, cameraRotation);

            if (states.IsKeyDown(Keys.D)) // Right?
                translateDirection += Vector3.TransformNormal(Vector3.Right, cameraRotation);

            // Now we modify the position by the calculated amount.
            Vector3 newPosition = position;
            if (translateDirection.LengthSquared() > 0) // Kinda pointless, as it won't ever be less than 0...
                newPosition += Vector3.Normalize(translateDirection) * distance;

            // Finally, we set the view matrix based on the new values.
            this.View = Matrix.CreateLookAt(newPosition, newPosition + newForward, Vector3.Up);
        }

        /**
         * Warp the cursor to the center of the window.
         */
        private void CenterCursor()
        {
            GameWindow window = _game.Window;

            Mouse.SetPosition(window.ClientBounds.Width / 2, window.ClientBounds.Height / 2);
        }

        #region Properties

        public Matrix View
        {
            get;
            set;
        }

        public Matrix Projection
        {
            get;
            set;
        }

        #endregion

    }
}
