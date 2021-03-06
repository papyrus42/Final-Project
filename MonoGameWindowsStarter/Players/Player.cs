﻿using System;
using Elemancy.Parallax;
using Elemancy.Transitions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Elemancy
{
    /// <summary>
    /// An enumeration of possible player veritcal movement states
    /// </summary>
    public enum VerticalMovementState
    {
        OnGround,
        Jumping,
        DoubleJump,
        Falling,
    }

    /// <summary>
    /// For which direction the player is facing
    /// </summary>
    public enum Direction
    {
        West = 2,
        East,
        Idle
    }

    /// <summary>
    /// For the current 'Elemental Power' of the player
    /// </summary>
    public enum Element
    {
        None = 0,
        Fire = 1,
        Water = 2,
        Lightning = 3,        
    }

    public class Player : ISprite
    {
        // Timers for fading and flickering when dying and being hit
        InterpolationTimer fade;
        InterpolationTimer flicker;
        float multiple;

        // How much the animation moves per frames 
        const int FRAME_RATE = 124;

        // The duration of a player's jump, in milliseconds
        const int JUMP_TIME = 500;

        // The speed of the player
        public const float PLAYER_SPEED = 125;

        // The speed that the player falls
        public const float FALL_SPEED = 200;

        // Width of animation frames
        public const int FRAME_WIDTH = 215;

        // height of animation frames
        const int FRAME_HEIGHT = 289;

        // The player's vertical movement state
        VerticalMovementState verticalState;

        // The player's facing direction
        Direction direction;

        // A timer for jumping
        TimeSpan jumpTimer;

        Animation walkAnimation;
        Texture2D playerIdle;

        Animation fireWalkAnimation;
        Animation waterWalkAnimation;
        Animation lightningWalkAnimation;

        Texture2D fireIdle;
        Texture2D waterIdle;
        Texture2D lightningIdle;


        // Old keyboard state for in Update
        KeyboardState oldState;

        // 'Elemental Power' state of the player
        public Element Element = Element.None;

        // 'Elemental Power' Orb
        public ElementalOrb elementalOrb { get; }

        // The Game 
        Game game;

        bool deathQuipped = false;

        /// <summary>
        /// The Player's position
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// The Bounds of the Player
        /// </summary>
        public BoundingRectangle Bounds;

        /// <summary>
        /// The color of the player: white for lightning, red for fire, blue for water
        /// </summary>
        public Color Color;

        public int HitDamage;

        /// <summary>
        /// the health of the player, need to bind with healthbar
        /// </summary>
        public int Health { get; protected set; }

        public bool IsDead { get; set; } = false;

        public bool IsHit { get; set; } = false;

        /// <summary>
        /// Constructing the Player
        /// </summary>
        /// <param name="game">The game the Player belongs to</param>
        /// <param name="player">The Texture</param>
        /// <param name="position">The Position</param>
        /// <param name="health">The Player's starting health</param>
        public Player(Game game, Color color)
        {
            this.game = game;
            this.Color = color;
            elementalOrb = new ElementalOrb(game, this);
        }

        public void Initialize()
        {
            // For testing purposes
            Position = new Vector2(40,450);  // Start position could change with preference
            Health = 200; // Could also change with preference
            direction = Direction.Idle;
            verticalState = VerticalMovementState.OnGround;
            Bounds.Width = FRAME_WIDTH;
            Bounds.Height = FRAME_HEIGHT;

            IsDead = false;
            IsHit = false;

            flicker = new InterpolationTimer(TimeSpan.FromSeconds(0.25), 0.0f, 1.0f);
            fade = new InterpolationTimer(TimeSpan.FromSeconds(2), 1.0f, 0.0f);
            multiple = 1;

            Element = Element.None;
            elementalOrb.Initialize();
        }

        public void LoadContent(ContentManager content)
        { 
            fireIdle = content.Load<Texture2D>("Sprites/Player/Player-Idle-Fire");
            waterIdle = content.Load<Texture2D>("Sprites/Player/Player-Idle-Water");
            lightningIdle = content.Load<Texture2D>("Sprites/Player/Player-Idle-Lightning");
            Texture2D[] fireWalk = new Texture2D[8];
            Texture2D[] waterWalk = new Texture2D[8];
            Texture2D[] lightningWalk = new Texture2D[8];
            for (int i = 0; i < 8; i++) {
                fireWalk[i] = content.Load<Texture2D>("Sprites/Player/Player-Run-Fire-000" + i);
                waterWalk[i] = content.Load<Texture2D>("Sprites/Player/Player-Run-Water-000" + i);
                lightningWalk[i] = content.Load<Texture2D>("Sprites/Player/Player-Run-Lightning-000" + i);
            }
            fireWalkAnimation = new Animation(fireWalk, 8);
            waterWalkAnimation = new Animation(waterWalk, 8);
            lightningWalkAnimation = new Animation(lightningWalk, 8);

            playerIdle = fireIdle;
            walkAnimation = fireWalkAnimation;

            elementalOrb.LoadContent(content);
        }

        public void Update(GameTime gameTime)
        {
            //Movement
            KeyboardState keyboard = Keyboard.GetState();
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Bounds.X = Position.X;
            Bounds.Y = Position.Y;

            // So the player can't go backwards, would need to change as they 
            // progress through the levels
            if (Position.X < 40)
            {
                Position.X = 40;
            }

            // Vertical movement
            switch (verticalState)
            {
                case VerticalMovementState.OnGround:
                    if (keyboard.IsKeyDown(Keys.Up))
                    {
                        verticalState = VerticalMovementState.Jumping;
                        jumpTimer = new TimeSpan(0);
                    }
                    if (keyboard.IsKeyDown(Keys.Up) && oldState.IsKeyUp(Keys.Up))
                    {
                        verticalState = VerticalMovementState.DoubleJump;
                        jumpTimer = new TimeSpan(0);
                    }
                    break;
                case VerticalMovementState.Jumping:
                    jumpTimer += gameTime.ElapsedGameTime;
                    // Simple jumping and start fallings right after
                    Position.Y -= (600 / (float)jumpTimer.TotalMilliseconds);
                    if (jumpTimer.TotalMilliseconds >= JUMP_TIME)
                        verticalState = VerticalMovementState.Falling;
                    break;
               case VerticalMovementState.DoubleJump:
                    jumpTimer += gameTime.ElapsedGameTime;
                    // Simple jumping and start fallings right after
                    Position.Y -= (900 / (float)jumpTimer.TotalMilliseconds);
                    if (jumpTimer.TotalMilliseconds >= JUMP_TIME)
                        verticalState = VerticalMovementState.Falling;
                    break;
                case VerticalMovementState.Falling:
                    Position.Y += delta * FALL_SPEED;
                    // Come back to the ground
                    if (Position.Y > 450)
                    {
                        Position.Y = 450;
                        verticalState = VerticalMovementState.OnGround;
                    }
                    break;                 
            }

            if(IsHit)
            {
                // for when the player collides with the enenmy
                Position.X -= 200 * delta;
                direction = Direction.East;

                if (flicker.TimeElapsed.TotalSeconds >= 0.20)
                {
                    flicker.Stop();
                    flicker = new InterpolationTimer(TimeSpan.FromSeconds(0.25), 0.0f, 1.0f);
                    IsHit = false;
                    multiple = 1.0f;
                }
                else
                {
                    if (!flicker.IsRunning)
                        flicker.Start();

                    if (flicker.IsRunning)
                        flicker.Update(gameTime.ElapsedGameTime);

                    multiple = flicker.CurrentValue;
                }    
            }

            if (IsDead)
            {
                if (fade.TimeElapsed.TotalSeconds >= 1.75)
                {
                    fade.Stop();
                    multiple = 0;
                }

                if (!fade.IsRunning && multiple != 0)
                {
                    fade.Start();
                }
                else if(multiple != 0)
                {
                    if (fade.IsRunning)
                        fade.Update(gameTime.ElapsedGameTime);

                    multiple = fade.CurrentValue;
                }        
            }

            if (keyboard.IsKeyDown(Keys.Left))
            {
                if (verticalState == VerticalMovementState.Jumping || verticalState == VerticalMovementState.Falling)
                    direction = Direction.West;                       
                else
                    direction = Direction.West;
                Position.X -= delta * PLAYER_SPEED;
            }
            else if (keyboard.IsKeyDown(Keys.Right))
            {
                if (verticalState == VerticalMovementState.Jumping || verticalState == VerticalMovementState.Falling)
                    direction = Direction.East;            
                else
                    direction = Direction.East;
                Position.X += delta * PLAYER_SPEED;
            }
            else
            {
                direction = Direction.Idle;
            }

            if (keyboard.IsKeyDown(Keys.K) && !oldState.IsKeyDown(Keys.K))
            {
                UpdateHealth(Health);
            }

            // Elemental Orb Activate and Update
            if (keyboard.IsKeyDown(Keys.Space) && !oldState.IsKeyDown(Keys.Space) && elementalOrb.State == ElementalOrb.ActiveState.Idle)
            {
                Vector2 orbVelocity = new Vector2(1, 0);
                switch (direction)
                {
                    case Direction.East:
                        orbVelocity = new Vector2(1, 0);
                        break;
                    case Direction.West:
                        orbVelocity = new Vector2(-1, 0);
                        break;
                    case Direction.Idle:
                        orbVelocity = new Vector2(1, 0);
                        break;
                }

                elementalOrb.Attack(Position, orbVelocity, Element);
                SetDamage(game.GameState, Element);
            }
            elementalOrb.Update(gameTime);
            if (keyboard.IsKeyDown(Keys.LeftAlt) && !oldState.IsKeyDown(Keys.LeftAlt) && elementalOrb.State == ElementalOrb.ActiveState.Idle)
            {
                CycleElement();
            }
            oldState = keyboard;
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            
            if (direction == Direction.Idle) {
                spriteBatch.Draw(playerIdle, Position, Color * multiple);
            } 
            else walkAnimation.Draw(spriteBatch, gameTime, Position, direction == Direction.West, Color, multiple);
            elementalOrb.Draw(spriteBatch, gameTime);
        }

        /// <summary>
        /// Update the player's health. Currently only for damaging the player.
        /// </summary>
        /// <param name="damage">The damage done to the player's health.</param>
        public void UpdateHealth(int damage)
        {           
            Health -= damage;
            if(Health <= 0)
            {
                IsDead = true;
                IsHit = false;
                if (!deathQuipped)
                {
                    game.narrator.playDeathQuip();
                    deathQuipped = true;
                }
            }
            else
            {
                deathQuipped = false;
            }
        }

        public void SetDamage(GameState level, Element element)
        {
            if(level == GameState.Forest && element == Element.Fire)
            {
                HitDamage = 20;
            }
            else if(level == GameState.Cave && element == Element.Water)
            {
                HitDamage = 25;
            }
            else if(level == GameState.Dungeon && element == Element.Lightning)
            {
                HitDamage = 30;
            }
            else
            {
                HitDamage = 15;
            }
        }

        /// <summary>
        /// Method for switching element type - Probably using just for testing
        /// </summary>
        public void CycleElement()
        {
            if (Element == Element.None || Element == Element.Lightning) {
                SetElement(Element.Fire);
            } else if (Element == Element.Fire) {
                SetElement(Element.Water);
            } else if (Element == Element.Water) {
                SetElement(Element.Lightning);
            }
        }

        public void SetElement(Element elem) {
            if (elem == Element.None || elem == Element.Fire) {
                Element = Element.Fire;
                playerIdle = fireIdle;
                walkAnimation = fireWalkAnimation;
            } else if (elem == Element.Water) {
                Element = Element.Water;
                playerIdle = waterIdle;
                walkAnimation = waterWalkAnimation;
            } else if (elem == Element.Lightning) {
                Element = Element.Lightning;
                playerIdle = lightningIdle;
                walkAnimation = lightningWalkAnimation;
            }
        }
    }
}
