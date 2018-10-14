namespace Divert_Compiler
{
    partial class fmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtHome = new System.Windows.Forms.TextBox();
            this.btnFind = new System.Windows.Forms.Button();
            this.numRWYmin = new System.Windows.Forms.NumericUpDown();
            this.numPCNmin = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.numRadius = new System.Windows.Forms.NumericUpDown();
            this.lblInc = new System.Windows.Forms.Label();
            this.txtIncludeFields = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numRWYmin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPCNmin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRadius)).BeginInit();
            this.SuspendLayout();
            // 
            // txtHome
            // 
            this.txtHome.Location = new System.Drawing.Point(84, 12);
            this.txtHome.Name = "txtHome";
            this.txtHome.Size = new System.Drawing.Size(75, 22);
            this.txtHome.TabIndex = 0;
            // 
            // btnFind
            // 
            this.btnFind.Location = new System.Drawing.Point(84, 249);
            this.btnFind.Name = "btnFind";
            this.btnFind.Size = new System.Drawing.Size(75, 23);
            this.btnFind.TabIndex = 1;
            this.btnFind.Text = "FIND";
            this.btnFind.UseVisualStyleBackColor = true;
            this.btnFind.Click += new System.EventHandler(this.btnFind_Click);
            // 
            // numRWYmin
            // 
            this.numRWYmin.Location = new System.Drawing.Point(84, 193);
            this.numRWYmin.Maximum = new decimal(new int[] {
            25000,
            0,
            0,
            0});
            this.numRWYmin.Name = "numRWYmin";
            this.numRWYmin.Size = new System.Drawing.Size(75, 22);
            this.numRWYmin.TabIndex = 2;
            this.numRWYmin.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            // 
            // numPCNmin
            // 
            this.numPCNmin.Location = new System.Drawing.Point(84, 221);
            this.numPCNmin.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numPCNmin.Name = "numPCNmin";
            this.numPCNmin.Size = new System.Drawing.Size(75, 22);
            this.numPCNmin.TabIndex = 3;
            this.numPCNmin.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 17);
            this.label1.TabIndex = 4;
            this.label1.Text = "Home";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 195);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "Min RWY";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 223);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 17);
            this.label3.TabIndex = 6;
            this.label3.Text = "Min PCN";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 167);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 17);
            this.label4.TabIndex = 8;
            this.label4.Text = "Radius";
            // 
            // numRadius
            // 
            this.numRadius.Location = new System.Drawing.Point(84, 165);
            this.numRadius.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.numRadius.Name = "numRadius";
            this.numRadius.Size = new System.Drawing.Size(75, 22);
            this.numRadius.TabIndex = 7;
            this.numRadius.Value = new decimal(new int[] {
            250,
            0,
            0,
            0});
            // 
            // lblInc
            // 
            this.lblInc.AutoSize = true;
            this.lblInc.Location = new System.Drawing.Point(11, 43);
            this.lblInc.Name = "lblInc";
            this.lblInc.Size = new System.Drawing.Size(107, 17);
            this.lblInc.TabIndex = 10;
            this.lblInc.Text = "Additional fields";
            // 
            // txtIncludeFields
            // 
            this.txtIncludeFields.Location = new System.Drawing.Point(15, 63);
            this.txtIncludeFields.Multiline = true;
            this.txtIncludeFields.Name = "txtIncludeFields";
            this.txtIncludeFields.Size = new System.Drawing.Size(144, 96);
            this.txtIncludeFields.TabIndex = 9;
            // 
            // fmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(170, 284);
            this.Controls.Add(this.lblInc);
            this.Controls.Add(this.txtIncludeFields);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.numRadius);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numPCNmin);
            this.Controls.Add(this.numRWYmin);
            this.Controls.Add(this.btnFind);
            this.Controls.Add(this.txtHome);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "fmMain";
            this.Text = "Divert";
            this.Load += new System.EventHandler(this.fmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numRWYmin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPCNmin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRadius)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtHome;
        private System.Windows.Forms.Button btnFind;
        private System.Windows.Forms.NumericUpDown numRWYmin;
        private System.Windows.Forms.NumericUpDown numPCNmin;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numRadius;
        private System.Windows.Forms.Label lblInc;
        private System.Windows.Forms.TextBox txtIncludeFields;
    }
}

