﻿using Elemancy.Parallax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Elemancy.Transitions
{
    public class CaveLevel
    {
        private Game game;

        private Messages message;

        public List<IEnemy> caveEnemies = new List<IEnemy>();
        public EnemyBoss caveBoss;
        public IEnemy ActiveEnemy;

        // the enemy's health bar
        HealthBar enemyHealth, enemyGauge;

        private Random random = new Random();

        ParallaxLayer caveLayer;
        public CaveLevel(Game game)
        {
            this.game = game;
            message = new Messages(game);

            enemyHealth = new HealthBar(game, new Vector2(822, 0), Color.Gray);  //Top right corner
            enemyGauge = new HealthBar(game, new Vector2(822, 0), Color.Red);
        }

        public void LoadContent(ContentManager content)
        {
            message.LoadContent(content);
            enemyHealth.LoadContent(content);
            enemyGauge.LoadContent(content);

            caveLayer = new ParallaxLayer(game);

            float offset = 200;
            for (int i = 0; i < 10; i++)
            {

                BasicEnemy caveEnemy = new BasicEnemy(game, GameState.Cave, new Vector2(4500 + offset, 500));
                caveEnemy.LoadContent(content);
                caveLayer.Sprites.Add(caveEnemy);
                caveEnemies.Add(caveEnemy);
                offset += random.Next(200, 300);
            }

            caveBoss = new EnemyBoss(game, GameState.Cave, new Vector2(7500, 280));
            caveBoss.LoadContent(content);
            caveLayer.Sprites.Add(caveBoss);
            caveEnemies.Add(caveBoss);

            ActiveEnemy = caveEnemies[0];
            ActiveEnemy.IsActive = true;

            game.Components.Add(caveLayer);
            caveLayer.DrawOrder = 2;

            caveLayer.ScrollController = new TrackingPlayer(game.player, 1.0f);
        }

        /// <summary>
        /// Update enemies for this level.
        /// </summary>
        /// <param name="gameTime">The Game's gameTime</param>
        public void Update(GameTime gameTime)
        {

            if (ActiveEnemy.Hit)
            {
                enemyGauge.Update(gameTime, ActiveEnemy.Health, game.player.HitDamage);
                ActiveEnemy.Hit = false;
                ActiveEnemy.UpdateHealth(game.player.HitDamage);
            }

            if (ActiveEnemy.Dead)
            {
                caveEnemies[0].IsActive = false; // Don't draw the old one
                if (caveEnemies.Count > 1)
                {
                    caveEnemies.RemoveAt(0);
                    if (caveEnemies.Count == 0)
                    {
                        ActiveEnemy = caveBoss;
                        caveEnemies[0].IsActive = true;
                        if (caveBoss.Health <= 0)
                        {
                            caveBoss.Dead = true;
                        }
                    }
                    else
                    {
                        ActiveEnemy = caveEnemies[0];
                        caveEnemies[0].IsActive = true; // Draw active enemy
                    }
                    enemyGauge.RestartHealth(); // for the next enemy;
                }
            }

            ActiveEnemy.Update(game.player, gameTime);
        }

        /// <summary>
        /// Will need to pass in componentsBatch, I think
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            enemyHealth.Draw(spriteBatch);
            enemyGauge.Draw(spriteBatch);

            if (caveBoss.Dead)
            {
                message.SetMessage(2, out game.player.Position.X);
                message.Update(gameTime);
                if (message.Continue == false)
                {
                    game.TransitionDungeon = false;
                    message.Draw(spriteBatch, game.graphics);
                }
                else if (message.Continue == true)
                {
                    game.TransitionDungeon = true;
                    message.Continue = false;
                }
            }
            else if (game.player.IsDead)
            {
                message.SetMessage(-1, out game.player.Position.X);
                message.Update(gameTime);
                if (!message.BackMenu)
                {
                    message.Draw(spriteBatch, game.graphics);
                }
                else if (message.BackMenu)
                {
                    game.menu.Start = false;
                    game.music.SetGameState(game.player, false);
                    game.GameState = GameState.MainMenu;
                    game.Restart = true;                
                }
                else
                {
                    game.Exit();
                }

                // So it shows the message again and doesn't skip right to MainMenu
                if (game.GameState == GameState.MainMenu)
                {
                    message.BackMenu = false;
                }
            }
        }

        /// <summary>
        /// Reloads all basic enemies and boss enemies for the level.
        /// </summary>
        public void Reload()
        {
            while (caveLayer.Sprites.Count > 0)
            {
                caveLayer.Sprites.RemoveAt(0);
            }
            caveEnemies = new List<IEnemy>();
            float offset = 200;
            for (int i = 0; i < 10; i++)
            {

                BasicEnemy caveEnemy = new BasicEnemy(game, GameState.Cave, new Vector2(4500 + offset, 500));
                caveEnemy.LoadContent(game.Content);
                caveLayer.Sprites.Add(caveEnemy);
                caveEnemies.Add(caveEnemy);
                offset += random.Next(200, 300);
            }

            caveBoss = new EnemyBoss(game, GameState.Cave, new Vector2(7500, 400));
            caveBoss.LoadContent(game.Content);
            caveLayer.Sprites.Add(caveBoss);
            caveEnemies.Add(caveBoss);

            ActiveEnemy = caveEnemies[0];
            ActiveEnemy.IsActive = true;
            enemyGauge.RestartHealth();
        }

    }
}
