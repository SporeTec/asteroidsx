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

namespace asteroidsx
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";


            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            //graphics.IsFullScreen = true;
        }

        //  Vektoren für die Position und derzeitige Bewegung des Raumschiffs

        Vector2 spaceship_pos = new Vector2(100, 100);
        Vector2 spaceship_dir = new Vector2(0, 0);

        //  Vektoren für die Position und derzeitige Bewegung der Asteroiden

        List<Vector2> asteroid_pos = new List<Vector2>();
        List<Vector2> asteroid_dir = new List<Vector2>();

        // Spielwichtige Variablen für Leben Waffen und Kollision

        int lifes = 5;
        bool fire = false;
        int g_missile = 1000;
        double distance = 0.0f;
        float rotation = 0.0f;
        bool shipHit = false;
        Random random = new Random();
        int score = 0;

        // Texturen die geladen werden

        Texture2D spaceship;
        Texture2D asteroid;
        Texture2D planet;
        Texture2D gameover;
        Texture2D beam;
        SpriteFont font;

        
        // Sounds
        SoundEffect gameOverSound;
        SoundEffect end_exploSound;
        SoundEffect normal_exploSound;
        Song background;
        bool soundgoPlayed = true;
        bool soundeePlayed = true;
        bool soundnePlayed = true;
        bool soundbgPlayed = true;

        protected override void Initialize()
        {
            base.Initialize();

            for (int i = 0; i < 5; i++)
            {
                float x = (float)random.NextDouble() * (Window.ClientBounds.Width - asteroid.Width);
                float y = (float)random.NextDouble() * (Window.ClientBounds.Height - asteroid.Height);
                asteroid_pos.Add(new Vector2(x, y));

                float x_dir = (float)random.Next(-4, 4);
                float y_dir = (float)random.Next(-4, 4);
                asteroid_dir.Add(new Vector2(x_dir, y_dir));
            }

           

        }
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);


            spaceship = Content.Load<Texture2D>("ship4_1");
            asteroid = Content.Load<Texture2D>("asteroidBig01");
            planet = Content.Load<Texture2D>("Planet");
            gameover = Content.Load<Texture2D>("gameover");
            beam = Content.Load<Texture2D>("beam");

            font = Content.Load<SpriteFont>("Font");

            /*gameOverSound = Content.Load<SoundEffect>("death");
            end_exploSound = Content.Load<SoundEffect>("end_explo");
            normal_exploSound = Content.Load<SoundEffect>("normal_explo");
            background = Content.Load<Song>("background");*/
        }

        protected override void UnloadContent()
        {

        }


        protected override void Update(GameTime gameTime)
        {
            /*if (!soundbgPlayed)
            {
                MediaPlayer.Play(background);
                soundbgPlayed = true;
            }*/
            // Abfrage welche Tasten in diesem Frame gedrückt wurden

            KeyboardState keyboard = Keyboard.GetState();
            GamePadState gamePad = GamePad.GetState(PlayerIndex.One);

            // Beenden


            if (keyboard.IsKeyDown(Keys.Escape) ||
                gamePad.Buttons.Back == ButtonState.Pressed)
            {
                this.Exit();
            }

            if (keyboard.IsKeyDown(Keys.Enter) && (lifes <= 0))
            {
                Initialize();
                lifes = 5;
                score = 0;
                g_missile = 250;
                rotation = 0;
                spaceship_pos.X = Window.ClientBounds.Width / 2;
                spaceship_pos.Y = Window.ClientBounds.Height / 2 + 200;
                spaceship_dir = new Vector2(0, 0);
                shipHit = false;
                asteroid_pos.Clear();
                asteroid_dir.Clear();
                soundgoPlayed = false;
                soundbgPlayed = false;

                for (int i = 0; i < 5; i++)
                {
                    float x = (float)random.NextDouble() * (Window.ClientBounds.Width - asteroid.Width);
                    float y = (float)random.NextDouble() * (Window.ClientBounds.Height - asteroid.Height);
                    asteroid_pos.Add(new Vector2(x, y));

                    float x_dir = (float)random.Next(1, 4);
                    float y_dir = (float)random.Next(1, 4);
                    asteroid_dir.Add(new Vector2(x_dir, y_dir));
                }
            }

            if (lifes <= 0)
            {
                MediaPlayer.Stop();
                if (soundgoPlayed == false)
                {
                    gameOverSound.Play();
                    soundgoPlayed = true;
                }


                return;
            }

            // Links & Rechts 

            if ((keyboard.IsKeyDown(Keys.Left) || gamePad.DPad.Left == ButtonState.Pressed) && spaceship_dir.X > -4)
               
            {
                spaceship_dir.X -=1;
                rotation = (float)(1.5 * Math.PI);
            }
            if ((keyboard.IsKeyDown(Keys.Right) || gamePad.DPad.Right == ButtonState.Pressed) && spaceship_dir.X < 4)
            {
                spaceship_dir.X +=1;
                rotation = (float)(0.5 * Math.PI);
            }

            // hoch runter

            if ((keyboard.IsKeyDown(Keys.Up) || gamePad.DPad.Up == ButtonState.Pressed) && spaceship_dir.Y > -4)
            {
                spaceship_dir.Y -= 1;
                rotation = (float)(0);
            }

            if ((keyboard.IsKeyDown(Keys.Down) || gamePad.DPad.Down == ButtonState.Pressed) && spaceship_dir.Y < 4)
            {
                spaceship_dir.Y += 1;
                rotation = (float)(Math.PI);
            }

            spaceship_pos += spaceship_dir;

            shipHit = false;

            //Schubs - Strahl

            if ((keyboard.IsKeyDown(Keys.Space) && g_missile > 250))
            {
                fire = true;
                g_missile = 0;
            }

            g_missile++;

            // Kollision Rec Ship

            Rectangle ship_rec = new Rectangle((int)spaceship_pos.X, (int)spaceship_pos.Y, spaceship.Width, spaceship.Height);
            Rectangle beam_rec = new Rectangle((int)(spaceship_pos.X - spaceship.Width / 2 - beam.Width / 2), (int)(spaceship_pos.Y - spaceship.Height / 2 - beam.Height / 2), beam.Width, beam.Height);
            // Asteroiden an den Rändern respawnen lassen + Kollision Rec erschaffen und prüfen + pos um dir verschieben

            for (int i = 0; i < asteroid_pos.Count; i++)
            {
                if (asteroid_pos[i].X < 0)
                {
                    asteroid_pos[i] = new Vector2(Window.ClientBounds.Width, asteroid_pos[i].Y);
                }

                if (asteroid_pos[i].X > Window.ClientBounds.Width)
                {
                    asteroid_pos[i] = new Vector2(0, asteroid_pos[i].Y);
                }

                if (asteroid_pos[i].Y < 0)
                {
                    asteroid_pos[i] = new Vector2(asteroid_pos[i].X, Window.ClientBounds.Height);
                }

                if (asteroid_pos[i].Y > Window.ClientBounds.Height)
                {
                    asteroid_pos[i] = new Vector2(asteroid_pos[i].X, 0);
                }

                Rectangle asteroid_rec = new Rectangle((int)asteroid_pos[i].X, (int)asteroid_pos[i].Y, asteroid.Width, asteroid.Height);

                asteroid_pos[i] += asteroid_dir[i];

                if (fire && beam_rec.Intersects(asteroid_rec)) 
                {
                    Console.WriteLine("fired && intersecting");
                    asteroid_dir[i] = new Vector2(asteroid_dir[i].X + ((spaceship_pos.X - asteroid_pos[i].X)/ 50)*-1, (asteroid_dir[i].Y + (spaceship_pos.Y - asteroid_pos[i].Y)/50)*-1);
                }

                if (ship_rec.Intersects(asteroid_rec))
                {
                    shipHit = true;
                    lifes--;
                    spaceship_pos.X = Window.ClientBounds.Width / 2;
                    spaceship_pos.Y = Window.ClientBounds.Height / 2 + 200;
                    spaceship_dir = new Vector2(0, 0);
                    rotation = 0;
                    /*soundnePlayed = false;
                    if (!soundnePlayed)
                    {
                        normal_exploSound.Play();
                        soundnePlayed = true;
                    }*/
                }

               

                // distance - asteroid - planet

                distance = Math.Sqrt(Math.Pow(asteroid_pos[i].X - (Window.ClientBounds.Width / 2), 2d) + Math.Pow(asteroid_pos[i].Y - (Window.ClientBounds.Width / 2), 2d));

                if (distance < 150)
                {
                    asteroid_dir[i] = new Vector2((float)(((Window.ClientBounds.Width / 2) - asteroid_pos[i].X) / distance + asteroid_dir[i].X), (float)(((Window.ClientBounds.Height / 2) - asteroid_pos[i].Y) / distance + asteroid_dir[i].Y));
                }

                if (spaceship_pos.X < 0)
                {
                    spaceship_pos.X = Window.ClientBounds.Width;
                }

                if (spaceship_pos.X > Window.ClientBounds.Width)
                {
                    spaceship_pos.X = 0;
                }

                if (spaceship_pos.Y < 0)
                {
                    spaceship_pos.Y = Window.ClientBounds.Height;
                }

                if (spaceship_pos.Y > Window.ClientBounds.Height)
                {
                    spaceship_pos.Y = 0;
                }

                spaceship_dir = new Vector2(spaceship_dir.X * 0.996f, spaceship_dir.Y * 0.996f);

                if (distance < 100)
                {
                    score++;

                    int start_pos = random.Next(1, 4);

                    if (start_pos == 1)
                    {
                        asteroid_pos[i] = new Vector2(1, 1);
                    }

                    if (start_pos == 2)
                    {
                        asteroid_pos[i] = new Vector2(Window.ClientBounds.Width - 1, 1);
                    }

                    if (start_pos == 3)
                    {
                        asteroid_pos[i] = new Vector2(Window.ClientBounds.Height, 1);
                    }

                    if (start_pos == 4)
                    {
                        asteroid_pos[i] = new Vector2(Window.ClientBounds.Height - 1, Window.ClientBounds.Height - 1);
                    }

                    asteroid_dir[i] = new Vector2(random.Next(-4, 4), random.Next(-4, 4));
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            

            if (shipHit)
            {
                GraphicsDevice.Clear(Color.Red);
                shipHit = false;
                if (!soundeePlayed)
                {
                    end_exploSound.Play();
                    soundeePlayed = true;
                }
            }
            else
            {
                GraphicsDevice.Clear(Color.Black);
            }

            spriteBatch.Begin();

            

            if (fire)
            {
                fire = false;
                spriteBatch.Draw(this.beam, new Vector2(spaceship_pos.X - spaceship.Width / 2 - beam.Width / 2, spaceship_pos.Y - spaceship.Height / 2 - beam.Height / 2), Color.White);
            }

            foreach (Vector2 pos in asteroid_pos)
            {
                var draw_vec = new Vector2(pos.X - asteroid.Width / 2, pos.Y - asteroid.Height / 2);
                spriteBatch.Draw(this.asteroid, draw_vec, Color.White);
            }

            

            spriteBatch.Draw(this.planet, new Vector2(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2 + 100), null, Color.White, 0, new Vector2(planet.Width / 2, planet.Height / 2), 1.0f, SpriteEffects.None, 0);

            spriteBatch.Draw(this.spaceship, this.spaceship_pos, null, Color.White, rotation, new Vector2(spaceship.Width / 2, spaceship.Height / 2), 1.0f, SpriteEffects.None, 0);




            if (lifes <= 0)
            {
                spriteBatch.Draw(this.gameover, new Vector2(100, 0), Color.White);
                var size = font.MeasureString("Press Enter to restart");
                spriteBatch.DrawString(font, "Press Enter to restart",
                    new Vector2(400 - (size.X / 2), 550), Color.Red);
            }

            spriteBatch.DrawString(font, "Lifes: " + lifes + " Score : " + score + " Shield :" + g_missile, new Vector2(5f, 5f), Color.White);

            spriteBatch.End();


            base.Draw(gameTime);
        }
                 }
        }