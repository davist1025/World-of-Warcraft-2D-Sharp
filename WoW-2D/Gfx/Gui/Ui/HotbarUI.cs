﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoW_2D.Utils;

namespace WoW_2D.Gfx.Gui.Ui
{
    /// <summary>
    /// The hotbar ui.
    /// </summary>
    public class HotbarUI : UiControl
    {
        private const int slotCount = 11;
        private HotbarSlotUI[] slots;

        public HotbarUI(GraphicsDevice graphics) : base(graphics)
        {
            slots = new HotbarSlotUI[slotCount];
            for (int i = 0; i < slots.Length; i++)
                slots[i] = new HotbarSlotUI(graphics);

            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (i == 0)
                    slot.Position = new Vector2((graphics.Viewport.Width / 2 - slot.GetSize().Width / 2) - ((slots.Length * slot.GetSize().Width) / 2), graphics.Viewport.Height - slot.GetSize().Height);
                else
                {
                    var lastSlot = slots[i - 1];
                    slot.Position = new Vector2(lastSlot.Position.X + slot.GetSize().Width + 2f, lastSlot.Position.Y);
                }
            }
        }

        public override void LoadContent(ContentManager content)
        {}

        public override void Update()
        {}

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Global.ShouldHideUI)
            {
                spriteBatch.Begin(blendState: BlendState.NonPremultiplied);
                foreach (var slot in slots)
                    slot.Draw(spriteBatch);
                spriteBatch.End();
            }
        }
    }
}
