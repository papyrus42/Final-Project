﻿using Elemancy.Parallax;
using Elemancy.Transitions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Elemancy
{
    /// <summary>
    /// My TO DO:
    ///  1. Need to synch damage and player heaith with Healthbar width
    ///  
    /// Team left to do:
    ///  1. Adjust enemies and collision for player/enemy being hit and dying
    ///     > Create images for types of enemies
    ///  2. Include narrator when wanting to play it
    ///     > figure out when tot play and stop each wav
    ///  3. Adjust particles to follow ball
    ///  4. Adjust the player's health and enemies health to fit with health bar
    ///  5. Code for when Boss of specific level dies to show Transition
    ///  6. Include created drawn players
    ///  7. Uncomment and include scroll stop 
    ///  
    /// </summary>

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        /// <summary>
        /// Enemies
        /// </summary>
        List<IEnemy> forestEnemies = new List<IEnemy>();
        List<IEnemy> caveEnemies = new List<IEnemy>();
        List<IEnemy> dungeonEnemies = new List<IEnemy>();
        EnemyBoss forestBoss;
        EnemyBoss caveBoss;
        EnemyBoss dungeonBoss;
        IEnemy activeEnemy;

        public Player player;

        public GraphicsDeviceManager graphics;

        /// <summary>
        /// SpriteBatch is for Parallax Layers
        /// Player, Levels, etc.
        /// </summary>
        public SpriteBatch spriteBatch;

        /// <summary>
        /// Is for Game Components that don't move
        /// HealthBar, Messages, Transitions Screens, etc.
        /// </summary>
        SpriteBatch componentsBatch;
        Messages messages = new Messages();
        Menu menu;
        Level level = new Level();

        KeyboardState oldState;

        ParallaxLayer playerLayer, levelsLayer;
        TrackingPlayer playerT, levelsT;

        /// <summary>
        /// The enemy Health when enemy dies -> disappear
        /// When another enemy appears -> appear and start at full health
        /// </summary>
        HealthBar wizardHealth, wizardGauge;
        HealthBar enemyHealth, enemyGauge;

        // Basic Particle Stuff
        //Random random = new Random();
        //ParticleSystem particleSystem;
        //Texture2D particleTexture;

        private GameState gameState;
        int scroll = 3117; // first level

        public GameState GameState 
        { 
            get { return gameState; } 
            set 
            {
                gameState = value;
                gameState = level.SetGameState(player, gameState != GameState.MainMenu);              
            } 
        }

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            menu = new Menu(this);

            // Creating and Positioning Healthbars
            wizardHealth = new HealthBar(this, new Vector2(20, 0), Color.Gray);  //Top left corner
            wizardGauge = new HealthBar(this, new Vector2(20, 0), Color.Red);  

            enemyHealth = new HealthBar(this, new Vector2(822, 0), Color.Gray);  //Top right corner
            enemyGauge = new HealthBar(this, new Vector2(822, 0), Color.Red);
        
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
            graphics.PreferredBackBufferWidth = 1042;
            graphics.PreferredBackBufferHeight = 768;
            graphics.ApplyChanges();

            player.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            componentsBatch = new SpriteBatch(GraphicsDevice);

            wizardHealth.LoadContent(Content);
            wizardGauge.LoadContent(Content);

            enemyHealth.LoadContent(Content);
            enemyGauge.LoadContent(Content);

            menu.LoadContent(Content);
            messages.LoadContent(Content);
            level.LoadContent(Content);


            // Player Layer
            player = new Player(this, Color.White);
            player.LoadContent(Content);
            playerLayer = new ParallaxLayer(this);
            playerLayer.Sprites.Add(player);
            playerLayer.DrawOrder = 2;
            Components.Add(playerLayer);

            for (int i = 0; i < 10; i++)
            {
                //positions may be updated later when the enemies "spawn"
                forestEnemies.Add(new BasicEnemy(30, 5, "fire", this, new Vector2(player.Position.X + 100, 500)));
                caveEnemies.Add(new BasicEnemy(40, 10, "water", this, new Vector2(player.Position.X + 100, 500)));
                dungeonEnemies.Add(new BasicEnemy(50, 15, "lightning", this, new Vector2(player.Position.X + 100, 500)));
            }
            //determine where bosses are going to be placed in the level
            forestBoss = new EnemyBoss(60, 10, "fire", this, new Vector2(300, 700)); 
            caveBoss = new EnemyBoss(80, 20, "water", this, new Vector2(300, 700));   
            dungeonBoss = new EnemyBoss(100, 30, "lightning", this, new Vector2(300, 700));

            levelsLayer = new ParallaxLayer(this);
            // Levels Layer - Can just add to to them for other levels

            var levelTextures = new List<Texture2D>()
            {
               Content.Load<Texture2D>("forest1"),
               Content.Load<Texture2D>("forest2"),
               Content.Load<Texture2D>("forest1"), // 4167
               Content.Load<Texture2D>("cave1"),
               Content.Load<Texture2D>("cave2"),
               Content.Load<Texture2D>("cave1"),
               Content.Load<Texture2D>("dungeon1"),
               Content.Load<Texture2D>("dungeon2")
            };

            var position = Vector2.Zero;
            var levelSprites = new List<StaticSprite>();
            for (int i = 0; i < levelTextures.Count; i++)
            {
                if(i == 7) position = new Vector2((9 * 1389) - 50, 0);
                else  position = new Vector2((i * 1389) - 50, 0);

                var sprite = new StaticSprite(levelTextures[i], position);
                levelSprites.Add(sprite);
            }

            foreach (var sprite in levelSprites)
            {
                levelsLayer.Sprites.Add(sprite);
            }
           
            levelsLayer.DrawOrder = 1;
            Components.Add(levelsLayer);

            playerT = new TrackingPlayer(player, 1.0f);
            levelsT = new TrackingPlayer(player, 1.0f);

            playerLayer.ScrollController = playerT;
            levelsLayer.ScrollController = levelsT;
            GameState = GameState.MainMenu;

            //add for loop for enemies when we get texture files
            //Add Enemies to Components with DrawOrder so they appear on top of layers

            //load enemy content
            //enemies need to be added to the draw order
            for (int i = 0; i < forestEnemies.Count; i++)
            {
                //name of file will change, added a temp png for testing
                forestEnemies[i].LoadContent(Content, "tempEnemy");
                caveEnemies[i].LoadContent(Content, "tempEnemy");
                dungeonEnemies[i].LoadContent(Content, "tempEnemy");
            }

            forestBoss.LoadContent(Content, "tempEnemy");
            caveBoss.LoadContent(Content, "tempEnemy");
            dungeonBoss.LoadContent(Content, "tempEnemy");

            //setting the first active enemy to be the first enemy in the forest level
            activeEnemy = forestEnemies[0];
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // If player is hit Update, using Keyboard for now for testing purposes
            KeyboardState current = Keyboard.GetState();

            menu.Update(gameTime);

            
            switch (gameState)
            {
                case GameState.MainMenu:
                    menu.Update(gameTime);
                    break;
                default:

                    if (current.IsKeyDown(Keys.H))
                    {
                        player.IsHit = true;
                        // Minus the Health by the damage done when player was hit/Is collided with, using -1 for now
                        // Need to synch damage and player heaith with Healthbar width
                        wizardGauge.Bounds.Width -= 1;
                        player.UpdateHealth(1);
                    }

                    player.Update(gameTime);

                    if (player.Element == Element.None)
                    {
                        player.Element = menu.selectedElement;
                    }
                    
                    //enemy update
                    activeEnemy.Update(player, gameTime);
                    if (activeEnemy.dead)
                    {
                        if (forestEnemies.Count > 0)
                        {
                            forestEnemies.RemoveAt(0);
                            if (forestEnemies.Count == 0)
                            {
                                activeEnemy = forestBoss;
                            }
                            else activeEnemy = forestEnemies[0];
                        }
                        else if (caveEnemies.Count > 0)
                        {
                            caveEnemies.RemoveAt(0);
                            if (caveEnemies.Count == 0)
                            {
                                activeEnemy = caveBoss;
                            }
                            else activeEnemy = caveEnemies[0];
                        }
                        else if (dungeonEnemies.Count > 0)
                        {
                            dungeonEnemies.RemoveAt(0);
                            if (dungeonEnemies.Count == 0)
                            {
                                activeEnemy = dungeonBoss;
                            }
                            else activeEnemy = caveEnemies[0];
                        }

                    }

                    // Cheat way to get song to switch right now
                    if (player.Position.X >= 4120 && player.Position.X <= 8334 && !level.IsPLaying)
                    {
                        gameState = level.SetGameState(player, menu.Start);
                        level.IsPLaying = true;
                    }
                    if (player.Position.X >= 8334 && level.IsPLaying)
                    {
                        level.IsPLaying = false;
                        gameState = level.SetGameState(player, menu.Start);
                    }

                    scroll = level.GetScrollStop(gameState);

                    if (player.Position.X >= scroll)
                    {
                        levelsT.ScrollStop = scroll;
                        levelsT.ScrollRatio = 0.0f;
                        playerT.ScrollRatio = 0.0f;
                        // probably need to restrict the player's movement 
                        // So they have to battle the boss; 
                    }

                    break; // END OF DEFAULT
            }

           

            /**/

            // Transition screen will be shown when the boss for that level is 
            // dead, and then If they hit C for Continue will change Player position 
            // and  scroll will change.

            base.Update(gameTime);
            oldState = current;

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            spriteBatch.End();

            base.Draw(gameTime);

            componentsBatch.Begin();


            switch(gameState)
            {
                case GameState.MainMenu:
                    if (!menu.Start)
                    {
                        menu.Draw(componentsBatch, graphics);
                        //restart the game / re-initialize player at the beginning
                    }
                    break;
                default:

                    wizardHealth.Draw(componentsBatch);
                    wizardGauge.Draw(componentsBatch);

                    enemyHealth.Draw(componentsBatch);
                    enemyGauge.Draw(componentsBatch);

                    activeEnemy.Draw(componentsBatch, Color.White);
                    break;
            }
            

            //messages.Draw(componentsBatch, graphics);

            componentsBatch.End();
        }
    }
}
