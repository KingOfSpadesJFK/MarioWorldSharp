﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarioWorldSharp.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame;
using MarioWorldSharp.Levels;

namespace MarioWorldSharp.Entities
{
    public enum PowerupEnum
    {
        Small = 0,
        Mushroom = 1,
        Cape = 2,
        Fire = 3
    }

    public class Player : IEntity
    {
        private double _xPos;
        private double _yPos;
        public double XPosition
        {
            get => _xPos;
            set
            {
                _xPos = value;
                if (collisionBox != null)
                    collisionBox.X = (int)value;
            }
        }
        public double YPosition
        {
            get => _yPos;
            set
            {
                _yPos = value;
                if (collisionBox != null)
                    collisionBox.Y = (int)value + colDisp;
            }
        }
        public double XSpeed { get; set; }
        public double YSpeed { get; set; }
        public double FacingAngle { get; set; }
        public byte VertGravity { get; set; }
        public byte HorizGravity { get; set; }
        public bool BlockedBellow { get; set; }
        public bool BlockedAbove { get; set; }
        public bool BlockedLeft { get; set; }
        public bool BlockedRight { get; set; }
        public PowerupEnum Powerup { get; set; }
        public Texture2D[] Poses { get; set; }
        public int Pose { get; set; }
        public int YDrawDisplacement { get; set; }
        public byte DashTimer { get; set; }
        public EntityStatus Status { get; set; }
        public EntityData Data { get; set; }
        public bool SpinJumping { get => spinJumped; }

        private Rectangle collisionBox;
        private bool facingRight;
        private byte animationTimer;
        private int colDisp;
        private bool jumped;
        private bool dashJumped;
        private bool spinJumped;
        private bool flip;
        private bool debug;
        private bool debugged;
        private bool ducking;

        public Player() : this(0, 0)
        {}

        private readonly int heightDisp = 0;

        public Player(double XPosition, double YPosition)
        {
            facingRight = true;
            VertGravity = 1;
            HorizGravity = 0;
            if (!Powerup.Equals(PowerupEnum.Small))
            {
                YDrawDisplacement = 0;
                colDisp = heightDisp;
                collisionBox = new Rectangle(0, 0, 16, 32 - heightDisp);
            }
            else
            {
                YDrawDisplacement = 9;
                colDisp = 16 + heightDisp;
                collisionBox = new Rectangle(0, 0, 16, 16 - heightDisp);
            }
            DashTimer = 0;
            Pose = 0;

            this.XPosition = XPosition;
            this.YPosition = YPosition;
            Console.WriteLine("It's a me!");
            Console.WriteLine(this.GetType());
            Poses = new Texture2D[70];

            //Input Events
            SMW.InputEvent.JumpPressEvent += Jump;
            SMW.InputEvent.SpinPressEvent += Spinjump;
        }

        public Rectangle GetCollisionBox() { return collisionBox; }

        public void Process()
        {
            if (Poses.GetLength(0) < 70)
                throw new FormatException();
            if (Keyboard.GetState().IsKeyDown(Keys.F))
            {
                if (!debugged)
                {
                    if (!debug)
                        debug = true;
                    else
                        debug = false;
                    debugged = true;
                }
            }
            else
                debugged = false;

            if (debug)
            {
                double move = 2.0;
                var keyState = Keyboard.GetState();
                if (keyState.IsKeyDown(Keys.A))
                    move = 6.0;
                if (keyState.IsKeyDown(Keys.Left))
                    XPosition -= move;
                if (keyState.IsKeyDown(Keys.Right))
                    XPosition += move;
                if (keyState.IsKeyDown(Keys.Up))
                    YPosition -= move;
                if (keyState.IsKeyDown(Keys.Down))
                    YPosition += move;
            }
            else
            {
                EnvironmentCollision();
                Move();
                UpdateXPositionition();
                UpdateYPositionition();
            }
        }

        public Texture2D GetTexture()
        {
            return Poses[Pose];
        }

        public static readonly int SideVertColisionOffset = 5;
        public static readonly int SideHorizCollisionOffset = 3;
        public static readonly int TopBotHorizCollisionOffset = 8;
        private void EnvironmentCollision()
        {
            BlockedAbove = false;
            BlockedBellow = false;
            BlockedLeft = false;
            BlockedRight = false;

            //Check left collision
            SMW.Level.GetBlockFromPosition(collisionBox.Left + SideHorizCollisionOffset, collisionBox.Top + TopBotHorizCollisionOffset).Left(this, collisionBox.Left + SideHorizCollisionOffset, collisionBox.Top + TopBotHorizCollisionOffset);
            SMW.Level.GetBlockFromPosition(collisionBox.Left + SideHorizCollisionOffset, collisionBox.Bottom - TopBotHorizCollisionOffset).Left(this, collisionBox.Left + SideHorizCollisionOffset, collisionBox.Bottom - TopBotHorizCollisionOffset);

            //Check right collision
            SMW.Level.GetBlockFromPosition(collisionBox.Right - SideHorizCollisionOffset, collisionBox.Top + TopBotHorizCollisionOffset).Right(this, collisionBox.Right - SideHorizCollisionOffset, collisionBox.Top + TopBotHorizCollisionOffset);
            SMW.Level.GetBlockFromPosition(collisionBox.Right - SideHorizCollisionOffset, collisionBox.Bottom - TopBotHorizCollisionOffset).Right(this, collisionBox.Right - SideHorizCollisionOffset, collisionBox.Bottom - TopBotHorizCollisionOffset);

            //Check bottom collision
            SMW.Level.GetBlockFromPosition(collisionBox.Left + SideVertColisionOffset, collisionBox.Bottom).Bellow(this, collisionBox.Left + SideVertColisionOffset, collisionBox.Bottom);
            SMW.Level.GetBlockFromPosition(collisionBox.Right - SideVertColisionOffset, collisionBox.Bottom).Bellow(this, collisionBox.Right - SideVertColisionOffset, collisionBox.Bottom);

            //Check top collision
            SMW.Level.GetBlockFromPosition(collisionBox.Left + SideVertColisionOffset, collisionBox.Top).Above(this, collisionBox.Left + SideVertColisionOffset, collisionBox.Top);
            SMW.Level.GetBlockFromPosition(collisionBox.Right - SideVertColisionOffset, collisionBox.Top).Above(this, collisionBox.Right - SideVertColisionOffset, collisionBox.Top);
        }

        private void UpdateXPositionition()
        {
            XPosition += XSpeed;
        }

        private void UpdateYPositionition()
        {
            double gravity = VertGravity * .375;
            if (Input.Jump.IsKeyHeld() || Input.Spinjump.IsKeyHeld())
                gravity = VertGravity * 0.1875;
            if (YSpeed < 64.0 / 16.0)
                YSpeed += gravity;
            YPosition += YSpeed;
        }

        private void Jump(object sender, EventArgs e)
        {
            if (!jumped && BlockedBellow)
            {
                //SMW Jump Velocity formula
                //BaseJumpVelocity - (640 * |XSpeed * 16 / 4| / 256)
                YSpeed = (-80.0 - (640.0 * Math.Abs(XSpeed * 1.5) / 256.0)) / 16.0;
                Jumping();
            }
        }

        private void Jumping()
        {
            jumped = true;
            if (DashTimer >= 112)
                dashJumped = true;
        }

        private void Spinjump(object sender, EventArgs e)
        {
            if (!jumped && BlockedBellow)
            {
                YSpeed = (-74.0 - ((592.0 * Math.Abs(XSpeed * 2.0)) / 256.0)) / 16.0;
                spinJumped = true;
                Jumping();
            }
        }

        private void Move()
        {
            //SMW velocity to double: v / 16.0
            double acceleration = 1.5 / 16.0;
            double decceleration = 640.0 / 256.0 / 16.0;
            double maxXSpeed = 20.0 / 16.0;

            if (BlockedBellow && jumped)
            {
                jumped = false;
                dashJumped = false;
                spinJumped = false;
            }

            if (BlockedBellow && Input.Down.IsKeyHeld())
                ducking = true;
            else if (BlockedBellow)
                ducking = false;

            bool notTwoHDir = !(Input.Left.IsKeyHeld() && Input.Right.IsKeyHeld());
            bool moving = (Input.Left.IsKeyHeld() || Input.Right.IsKeyHeld()) && !ducking && notTwoHDir;

            if (Input.Dash.IsKeyHeld())
            {
                acceleration = 1.5 / 16.0;
                decceleration = 1280.0 / 256.0 / 16.0;
                if (DashTimer >= 112)
                    maxXSpeed = 48.0 / 16.0;
                else
                    maxXSpeed = 36.0 / 16.0;
            }

            if (Input.Jump.IsKeyHeld() || Input.Spinjump.IsKeyHeld())
            {
                if (animationTimer == 0 && YSpeed <= 0 && !BlockedBellow)
                {
                    if (dashJumped)
                        Pose = 0x0C;
                    else
                        Pose = 0x0B;
                }
            }

            if (BlockedBellow && (moving || XSpeed != 0))
            {
                if (animationTimer == 0)
                {
                    Pose++;
                    if (DashTimer == 112)
                        Pose = (Pose % 2) + 4;
                    else
                        Pose %= 2;
                    if (Math.Abs(XSpeed) >= 48.0 / 16.0)
                        animationTimer = 2;
                    else if (Math.Abs(XSpeed) >= 36.0 / 16.0)
                        animationTimer = 3;
                    else if (Math.Abs(XSpeed) >= 20.0 / 16.0)
                        animationTimer = 6;
                    else
                        animationTimer = 10;
                }
            }
            else
            {
                if (BlockedBellow)
                    Pose = 0;
            }

            if (animationTimer > 0)
                animationTimer--;
            
            if (BlockedBellow && moving && Input.Dash.IsKeyHeld())
            {
                if (Math.Abs(XSpeed) >= 36.0 / 16.0)
                {
                    if (DashTimer < 112)
                        DashTimer += 2;
                    else
                        DashTimer = 112;
                }
                else
                    if (DashTimer > 0)
                    DashTimer--;
            }
            else
            {
                if (!BlockedBellow && Input.Dash.IsKeyHeld() && DashTimer >= 112)
                {
                    if (Math.Abs(XSpeed) < 48.0 / 16.0)
                        DashTimer--;
                }
                else if (DashTimer > 0)
                    DashTimer--;
            }

            if (!dashJumped && DashTimer < 112 && animationTimer == 0 && !BlockedBellow && (YSpeed > 0 || (YSpeed < 0 && !jumped)))
                Pose = 0x24;

            if (spinJumped && !BlockedBellow)
            {
                if (animationTimer == 0 || animationTimer > 7)
                    animationTimer = 7;
                int[] posePointers = { 0, 0x0F, 0, 0x39 };
                bool[] facingRightSpinning = { false, false, true, false };
                Pose = posePointers[(animationTimer - 1) / 2];
                flip ^= facingRightSpinning[(animationTimer - 1) / 2];
                ducking = false;
            }
            else
                flip = facingRight;

            if (XSpeed == 0 && !moving && BlockedBellow && Input.Up.IsKeyHeld())
                Pose = 0x03;

            if (ducking)
                Pose = 0x3C;

            if (!ducking && Powerup != PowerupEnum.Small)
            {
                colDisp = 0 + heightDisp;
                collisionBox.Height = 32;
            }
            else
            {
                colDisp = 16 + heightDisp;
                collisionBox.Height = 16;
            }

            moving = (Input.Left.IsKeyHeld() || Input.Right.IsKeyHeld()) && notTwoHDir;

            #region Moving
            if ((!BlockedBellow || !ducking) && notTwoHDir && moving)
            {
                if (Input.Left.IsKeyHeld())
                {
                    facingRight = false;
                    if (XSpeed > 0)
                    {
                        if (BlockedBellow)
                            Pose = 0x0D;
                        XSpeed -= decceleration;
                    }
                    else
                    {
                        if (XSpeed + acceleration > -maxXSpeed)
                            XSpeed -= acceleration;
                        else
                            XSpeed = -maxXSpeed;
                    }
                } 
                else if (Input.Right.IsKeyHeld())
                {
                    facingRight = true;
                    if (XSpeed < 0)
                    {
                        if (BlockedBellow)
                            Pose = 0x0D;
                        XSpeed += decceleration;
                    }
                    else
                    {
                        if (XSpeed + acceleration < maxXSpeed)
                            XSpeed += acceleration;
                        else
                            XSpeed = maxXSpeed;
                    }
                }
            }
            #endregion
            else
            {
                decceleration = 1.0 / 16.0;
                # region Friction (Decceleration only on ground)
                if (BlockedBellow && (!moving || ducking))
                {
                    if (XSpeed > 0)
                    {
                        if (XSpeed - decceleration <= 0)
                            XSpeed = 0;
                        else
                            XSpeed -= decceleration;
                    }
                    else if (XSpeed < 0)
                    {
                        if (XSpeed + decceleration >= 0)
                            XSpeed = 0;
                        else
                            XSpeed += decceleration;
                    }
                }
                #endregion
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(GetTexture(),
                new Rectangle((int)XPosition - (int)SMW.Level.X, (int)YPosition + YDrawDisplacement - (int)SMW.Level.Y, GetTexture().Width, GetTexture().Height),
                new Rectangle(0, 0, GetTexture().Width, GetTexture().Height),
                Color.White, 0.0F, Vector2.Zero, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1F);
        }

        public void Kill()
        {
            throw new NotImplementedException();
        }
    }
}
