﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework.Entity;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Framework.Entity.Vector;

namespace WoW_2D.World.GameObject
{
    /// <summary>
    /// A class for NPCs.
    /// </summary>
    public class NPC : IMob
    {
        public WorldCreature Info { get; set; }

        public override void Initialize()
        {
            Model = ModelManager.GetModel(m => m.ID == Info.ModelID);
            Model.Animations[0].IsActive = true;
        }

        public override void Update(GameTime gameTime)
        {
            switch (Info.Vector.Direction)
            {
                case MoveDirection.East:
                    SetAnimation(x => x.Name == "east_anim");
                    break;
                case MoveDirection.West:
                    SetAnimation(x => x.Name == "west_anim");
                    break;
                case MoveDirection.North:
                case MoveDirection.North_East:
                case MoveDirection.North_West:
                    SetAnimation(x => x.Name == "north_anim");
                    break;
                case MoveDirection.South:
                case MoveDirection.South_East:
                case MoveDirection.South_West:
                    SetAnimation(x => x.Name == "south_anim");
                    break;
            }

            GetAnimation().Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            bool idle = (!Info.IsMoving) ? true : false;
            SetAnimationIdle(idle);
            GetAnimation().Draw(spriteBatch, new Vector2(Info.Vector.X, Info.Vector.Y));
        }
    }
}