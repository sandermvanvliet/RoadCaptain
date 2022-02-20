﻿
namespace RoadCaptain.Host.Console
{
    partial class MainWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxAvailableCommands = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxAvailableTurns = new System.Windows.Forms.TextBox();
            this.textBoxCurrentDirection = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxCurrentSegment = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.skControl1 = new SkiaSharp.Views.Desktop.SKControl();
            this.panel2 = new System.Windows.Forms.Panel();
            this.pictureBoxTurnLeft = new System.Windows.Forms.PictureBox();
            this.pictureBoxGoStraight = new System.Windows.Forms.PictureBox();
            this.pictureBoxTurnRight = new System.Windows.Forms.PictureBox();
            this.menuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTurnLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxGoStraight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTurnRight)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(2297, 33);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(54, 29);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(141, 34);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.textBoxAvailableCommands);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.textBoxAvailableTurns);
            this.panel1.Controls.Add(this.textBoxCurrentDirection);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.textBoxCurrentSegment);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(12, 1227);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(986, 210);
            this.panel1.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 128);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(181, 25);
            this.label4.TabIndex = 7;
            this.label4.Text = "Available commands:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxAvailableCommands
            // 
            this.textBoxAvailableCommands.Location = new System.Drawing.Point(201, 125);
            this.textBoxAvailableCommands.Name = "textBoxAvailableCommands";
            this.textBoxAvailableCommands.ReadOnly = true;
            this.textBoxAvailableCommands.Size = new System.Drawing.Size(761, 31);
            this.textBoxAvailableCommands.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(63, 91);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(132, 25);
            this.label3.TabIndex = 5;
            this.label3.Text = "Available turns:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxAvailableTurns
            // 
            this.textBoxAvailableTurns.Location = new System.Drawing.Point(201, 88);
            this.textBoxAvailableTurns.Name = "textBoxAvailableTurns";
            this.textBoxAvailableTurns.ReadOnly = true;
            this.textBoxAvailableTurns.Size = new System.Drawing.Size(761, 31);
            this.textBoxAvailableTurns.TabIndex = 4;
            // 
            // textBoxCurrentDirection
            // 
            this.textBoxCurrentDirection.Location = new System.Drawing.Point(201, 51);
            this.textBoxCurrentDirection.Name = "textBoxCurrentDirection";
            this.textBoxCurrentDirection.ReadOnly = true;
            this.textBoxCurrentDirection.Size = new System.Drawing.Size(761, 31);
            this.textBoxCurrentDirection.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(47, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(148, 25);
            this.label2.TabIndex = 2;
            this.label2.Text = "Current direction:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxCurrentSegment
            // 
            this.textBoxCurrentSegment.Location = new System.Drawing.Point(201, 14);
            this.textBoxCurrentSegment.Name = "textBoxCurrentSegment";
            this.textBoxCurrentSegment.ReadOnly = true;
            this.textBoxCurrentSegment.Size = new System.Drawing.Size(761, 31);
            this.textBoxCurrentSegment.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(47, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(148, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "Current segment:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // skControl1
            // 
            this.skControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.skControl1.Location = new System.Drawing.Point(12, 36);
            this.skControl1.Name = "skControl1";
            this.skControl1.Size = new System.Drawing.Size(2273, 1173);
            this.skControl1.TabIndex = 3;
            this.skControl1.Text = "skControl1";
            this.skControl1.PaintSurface += new System.EventHandler<SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs>(this.skControl1_PaintSurface);
            this.skControl1.SizeChanged += new System.EventHandler(this.skControl1_SizeChanged);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.pictureBoxTurnRight);
            this.panel2.Controls.Add(this.pictureBoxGoStraight);
            this.panel2.Controls.Add(this.pictureBoxTurnLeft);
            this.panel2.Location = new System.Drawing.Point(1005, 1227);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(622, 210);
            this.panel2.TabIndex = 4;
            // 
            // pictureBoxTurnLeft
            // 
            this.pictureBoxTurnLeft.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxTurnLeft.Image")));
            this.pictureBoxTurnLeft.Location = new System.Drawing.Point(4, 4);
            this.pictureBoxTurnLeft.Name = "pictureBoxTurnLeft";
            this.pictureBoxTurnLeft.Size = new System.Drawing.Size(200, 200);
            this.pictureBoxTurnLeft.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxTurnLeft.TabIndex = 0;
            this.pictureBoxTurnLeft.TabStop = false;
            this.pictureBoxTurnLeft.Visible = false;
            // 
            // pictureBoxGoStraight
            // 
            this.pictureBoxGoStraight.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxGoStraight.Image")));
            this.pictureBoxGoStraight.Location = new System.Drawing.Point(208, 4);
            this.pictureBoxGoStraight.Name = "pictureBoxGoStraight";
            this.pictureBoxGoStraight.Size = new System.Drawing.Size(200, 200);
            this.pictureBoxGoStraight.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxGoStraight.TabIndex = 1;
            this.pictureBoxGoStraight.TabStop = false;
            this.pictureBoxGoStraight.Visible = false;
            // 
            // pictureBoxTurnRight
            // 
            this.pictureBoxTurnRight.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxTurnRight.Image")));
            this.pictureBoxTurnRight.Location = new System.Drawing.Point(416, 4);
            this.pictureBoxTurnRight.Name = "pictureBoxTurnRight";
            this.pictureBoxTurnRight.Size = new System.Drawing.Size(200, 200);
            this.pictureBoxTurnRight.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxTurnRight.TabIndex = 2;
            this.pictureBoxTurnRight.TabStop = false;
            this.pictureBoxTurnRight.Visible = false;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2297, 1449);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.skControl1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainWindow";
            this.Text = "RoadCaptain";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTurnLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxGoStraight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTurnRight)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxAvailableTurns;
        private System.Windows.Forms.TextBox textBoxCurrentDirection;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxCurrentSegment;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxAvailableCommands;
        private SkiaSharp.Views.Desktop.SKControl skControl1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox pictureBoxTurnLeft;
        private System.Windows.Forms.PictureBox pictureBoxTurnRight;
        private System.Windows.Forms.PictureBox pictureBoxGoStraight;
    }
}
