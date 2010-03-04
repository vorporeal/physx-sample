using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using StillDesign.PhysX;

namespace PhysXTest
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PhysXTest : Microsoft.Xna.Framework.Game
    {
        #region Variables

        GraphicsDeviceManager graphics;

        Core PhysXCore;
        Scene Scene;
        Camera _camera;

        enum TEST_TYPE
        {
            BOX,
            FLUBBER
        };

        #endregion

        const TEST_TYPE TEST = TEST_TYPE.FLUBBER;

        public PhysXTest()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        ~PhysXTest()
        {
            // Get rid of the scene.  This will also get rid of any physics objects associated with it.
            this.Scene.Dispose();

            // Get rid of the core.  This will terminate the physics processing.
            this.PhysXCore.Dispose();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            CoreDescription coreDesc = new CoreDescription();
            this.PhysXCore = new Core(coreDesc, new ConsoleOutputStream());

            var core = this.PhysXCore;
            core.SetParameter(PhysicsParameter.VisualizationScale, 2.0f);
            core.SetParameter(PhysicsParameter.VisualizeCollisionShapes, true);

            SimulationType hworsw = (core.HardwareVersion == HardwareVersion.None ? SimulationType.Software : SimulationType.Hardware);
            Console.WriteLine("PhysX Acceleration Type: " + hworsw);

            SceneDescription sceneDesc = new SceneDescription()
            {
                SimulationType = hworsw,
                Gravity = new Vector3(0.0f, -9.81f, 0.0f)
            };

            this.Scene = core.CreateScene(sceneDesc);

            // If there's a remote debugger, connect to it.
            core.Foundation.RemoteDebugger.Connect("localhost");

            // Create the camera.
            _camera = new Camera(this);

            // Let's create physics objects!
            InitializeObjects();
        }

        private void InitializeObjects()
        {
            switch(TEST)
            {
                // This test creates a box that falls from the sky and bounces on the ground plane.
                // Nothing special.
                case TEST_TYPE.BOX:
                    SimpleBoxTest();
                    break;

                // This test creates a box as a container, and then creates a sphere in the box and
                // plays around with doing stuff when collisions occur.
                case TEST_TYPE.FLUBBER:
                    FlubberTest();
                    break;

                // If none of the existing tests are chosen, create nothing.
                default: ;
            }
        }

        private void SimpleBoxTest()
        {
            // Let's create a simple material.
            Material defaultMaterial = this.Scene.DefaultMaterial;
            defaultMaterial.StaticFriction = 0.5f;
            defaultMaterial.DynamicFriction = 0.5f;
            defaultMaterial.Restitution = 0.5f;

            // First, let's create a ground plane for the box to bounce on.
            ActorDescription planeActor = new ActorDescription();
            planeActor.Shapes.Add(new PlaneShapeDescription(Vector3.Up, 0.0f));
            this.Scene.CreateActor(planeActor);

            // Let's create a box!
            // This describes the physics properties of the box.  We only need to make one
            // of these if it is a dynamic object.  Static objects don't need a BodyDescription.
            BodyDescription boxDesc = new BodyDescription();
            boxDesc.AngularDamping = 0.5f;
            boxDesc.LinearVelocity = Vector3.Zero;
            // This describes the size and positioning of the box.
            ActorDescription boxActor = new ActorDescription();
            boxActor.Shapes.Add(new BoxShapeDescription(new Vector3(2.0f)));
            boxActor.Density = 10.0f;
            boxActor.GlobalPose *= Matrix.CreateTranslation(new Vector3(0.0f, 20.0f, 0.0f));
            boxActor.BodyDescription = boxDesc;
            // Let's add it to the scene!
            this.Scene.CreateActor(boxActor);
        }

        private void FlubberTest()
        {
            // Let's set the default material to make things BOUNCY.
            Material defaultMaterial = this.Scene.DefaultMaterial;
            defaultMaterial.StaticFriction = 0.0f;
            defaultMaterial.DynamicFriction = 0.01f;
            defaultMaterial.Restitution = 1.0f;
            defaultMaterial.RestitutionCombineMode = CombineMode.Max;

            // Here's another box.  This will be a container for the flubber to bounce around in.
            ActorDescription boxActor = new ActorDescription();
            // The back piece of the box.
            boxActor.Shapes.Add(new BoxShapeDescription(new Vector3(40.0f, 40.0f, 2.0f))
                {
                    LocalPosition = new Vector3(0.0f, 0.0f, -19.0f)
                });
            // The front piece of the box.
            boxActor.Shapes.Add(new BoxShapeDescription(new Vector3(40.0f, 40.0f, 2.0f))
                {
                    LocalPosition = new Vector3(0.0f, 0.0f, 19.0f)
                });
            // The left piece of the box.
            boxActor.Shapes.Add(new BoxShapeDescription(new Vector3(2.0f, 40.0f, 40.0f))
                {
                    LocalPosition = new Vector3(-19.0f, 0.0f, 0.0f)
                });
            // The right piece of the box.
            boxActor.Shapes.Add(new BoxShapeDescription(new Vector3(2.0f, 40.0f, 40.0f))
                {
                    LocalPosition = new Vector3(19.0f, 0.0f, 0.0f)
                });
            // The top piece of the box.
            boxActor.Shapes.Add(new BoxShapeDescription(new Vector3(40.0f, 2.0f, 40.0f))
                {
                    LocalPosition = new Vector3(0.0f, 19.0f, 0.0f)
                });
            // The bottom piece of the box.
            boxActor.Shapes.Add(new BoxShapeDescription(new Vector3(40.0f, 2.0f, 40.0f))
                {
                    LocalPosition = new Vector3(0.0f, -19.0f, 0.0f)
                });
            boxActor.GlobalPose *= Matrix.CreateTranslation(new Vector3(0.0f, 20.0f, 0.0f));
            this.Scene.CreateActor(boxActor);

            // Now we'll create a sphere inside the box.  It will start at the center of the box with
            // a random initial velocity.
            Random rand = new Random();
            const double speed = 100.0f;
            BodyDescription flubberDesc = new BodyDescription();
            flubberDesc.LinearVelocity = new Vector3((float) rand.NextDouble(),
                                                     (float) rand.NextDouble(),
                                                     (float) rand.NextDouble());
            flubberDesc.LinearVelocity.Normalize();
            flubberDesc.LinearVelocity -= new Vector3(0.5f);
            flubberDesc.LinearVelocity *= (float)(speed);
            flubberDesc.AngularDamping = 0.0f;
            flubberDesc.LinearDamping = 0.0f;

            ActorDescription flubberActor = new ActorDescription();
            flubberActor.Shapes.Add(new SphereShapeDescription(2.0f));
            flubberActor.GlobalPose *= Matrix.CreateTranslation(new Vector3(0.0f, 20.0f, 0.0f));
            flubberActor.BodyDescription = flubberDesc;
            flubberActor.Density = 10.0f;
            this.Scene.CreateActor(flubberActor);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Nothing to load.
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // Nothing to unload.
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                ||
                Keyboard.GetState().IsKeyDown(Keys.Escape) == true)
                this.Exit();

            // Update the simulation.
            this.Scene.Simulate((float) gameTime.ElapsedGameTime.TotalSeconds);
            this.Scene.FlushStream();
            // Get the results.  This call blocks until the simulation is done updating.
            this.Scene.FetchResults(SimulationStatus.RigidBodyFinished, true);

            // Have the camera update itself based on user input.
            _camera.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightBlue);

            // Draw the debugging stuff.  This method was shamelessly ripped straight from the PhysX.Net sample.
            // In fact, the code in the PhysX.Net sample was converted from the original in nVidia's documentation.
            DrawDebug(gameTime);

            base.Draw(gameTime);
        }

        private void DrawDebug(GameTime gameTime)
        {
            this.GraphicsDevice.VertexDeclaration = new VertexDeclaration(this.GraphicsDevice, VertexPositionColor.VertexElements);

            BasicEffect debugEffect = new BasicEffect(this.GraphicsDevice, null);
            debugEffect.World = Matrix.Identity;
            debugEffect.View = this._camera.View;
            debugEffect.Projection = this._camera.Projection;

            DebugRenderable data = this.Scene.GetDebugRenderable();

            debugEffect.Begin();

            foreach (EffectPass pass in debugEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                if (data.PointCount > 0)
                {
                    DebugPoint[] points = data.GetDebugPoints();

                    this.GraphicsDevice.DrawUserPrimitives<DebugPoint>(PrimitiveType.PointList, points, 0, points.Length);
                }

                if (data.LineCount > 0)
                {
                    DebugLine[] lines = data.GetDebugLines();

                    VertexPositionColor[] vertices = new VertexPositionColor[data.LineCount * 2];
                    for (int x = 0; x < data.LineCount; x++)
                    {
                        DebugLine line = lines[x];

                        vertices[x * 2 + 0] = new VertexPositionColor(line.Point0, Int32ToColor(line.Color));
                        vertices[x * 2 + 1] = new VertexPositionColor(line.Point1, Int32ToColor(line.Color));
                    }

                    this.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertices, 0, lines.Length);
                }

                if (data.TriangleCount > 0)
                {
                    DebugTriangle[] triangles = data.GetDebugTriangles();

                    VertexPositionColor[] vertices = new VertexPositionColor[data.TriangleCount * 3];
                    for (int x = 0; x < data.TriangleCount; x++)
                    {
                        DebugTriangle triangle = triangles[x];

                        vertices[x * 3 + 0] = new VertexPositionColor(triangle.Point0, Int32ToColor(triangle.Color));
                        vertices[x * 3 + 1] = new VertexPositionColor(triangle.Point1, Int32ToColor(triangle.Color));
                        vertices[x * 3 + 2] = new VertexPositionColor(triangle.Point2, Int32ToColor(triangle.Color));
                    }

                    this.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, vertices, 0, triangles.Length);
                }

                pass.End();
            }

            debugEffect.End();
        }
        
        // Takes an int and pulls the color out of it, byte by byte.
        public static Color Int32ToColor(int color)
        {
            byte a = (byte)((color & 0xFF000000) >> 32);
            byte r = (byte)((color & 0x00FF0000) >> 16);
            byte g = (byte)((color & 0x0000FF00) >> 8);
            byte b = (byte)((color & 0x000000FF) >> 0);

            return new Color(r, g, b, a);
        }
    }
}
