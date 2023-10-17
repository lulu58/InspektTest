namespace Visutronik
{
    partial class DlgInstruction
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
            this.groupBoxImage = new System.Windows.Forms.GroupBox();
            this.lblImageParams = new System.Windows.Forms.Label();
            this.tbxImageParam3 = new System.Windows.Forms.TextBox();
            this.tbxImageParam2 = new System.Windows.Forms.TextBox();
            this.tbxImageParam1 = new System.Windows.Forms.TextBox();
            this.lblImageRegion = new System.Windows.Forms.Label();
            this.lblCamera = new System.Windows.Forms.Label();
            this.cbxCamName = new System.Windows.Forms.ComboBox();
            this.lblImageParam3 = new System.Windows.Forms.Label();
            this.lblImageParam2 = new System.Windows.Forms.Label();
            this.lblImageParam1 = new System.Windows.Forms.Label();
            this.cbxImageRegion = new System.Windows.Forms.ComboBox();
            this.groupBoxOperation = new System.Windows.Forms.GroupBox();
            this.lblCheckerType = new System.Windows.Forms.Label();
            this.lblFilterTyp = new System.Windows.Forms.Label();
            this.cbxCheckerTyp = new System.Windows.Forms.ComboBox();
            this.cbxFilterTyp = new System.Windows.Forms.ComboBox();
            this.lblOpParams = new System.Windows.Forms.Label();
            this.cbxOperator = new System.Windows.Forms.ComboBox();
            this.lblOpType = new System.Windows.Forms.Label();
            this.groupBoxEvaluation = new System.Windows.Forms.GroupBox();
            this.buttonTest = new System.Windows.Forms.Button();
            this.chkAuswertungAktiv = new System.Windows.Forms.CheckBox();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxInstruction = new System.Windows.Forms.GroupBox();
            this.tbxDescription = new System.Windows.Forms.TextBox();
            this.tbxName = new System.Windows.Forms.TextBox();
            this.tbxInstructionNumber = new System.Windows.Forms.TextBox();
            this.lblNumber = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblInstructionName = new System.Windows.Forms.Label();
            this.lblInstrParams = new System.Windows.Forms.Label();
            this.groupBoxImage.SuspendLayout();
            this.groupBoxOperation.SuspendLayout();
            this.groupBoxEvaluation.SuspendLayout();
            this.groupBoxInstruction.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxImage
            // 
            this.groupBoxImage.Controls.Add(this.lblImageParams);
            this.groupBoxImage.Controls.Add(this.tbxImageParam3);
            this.groupBoxImage.Controls.Add(this.tbxImageParam2);
            this.groupBoxImage.Controls.Add(this.tbxImageParam1);
            this.groupBoxImage.Controls.Add(this.lblImageRegion);
            this.groupBoxImage.Controls.Add(this.lblCamera);
            this.groupBoxImage.Controls.Add(this.cbxCamName);
            this.groupBoxImage.Controls.Add(this.lblImageParam3);
            this.groupBoxImage.Controls.Add(this.lblImageParam2);
            this.groupBoxImage.Controls.Add(this.lblImageParam1);
            this.groupBoxImage.Controls.Add(this.cbxImageRegion);
            this.groupBoxImage.Location = new System.Drawing.Point(344, 18);
            this.groupBoxImage.Name = "groupBoxImage";
            this.groupBoxImage.Size = new System.Drawing.Size(321, 193);
            this.groupBoxImage.TabIndex = 0;
            this.groupBoxImage.TabStop = false;
            this.groupBoxImage.Text = "Bildregion";
            this.groupBoxImage.Enter += new System.EventHandler(this.GroupBoxImage_Enter);
            // 
            // lblImageParams
            // 
            this.lblImageParams.BackColor = System.Drawing.SystemColors.ControlDark;
            this.lblImageParams.Location = new System.Drawing.Point(6, 157);
            this.lblImageParams.Name = "lblImageParams";
            this.lblImageParams.Size = new System.Drawing.Size(309, 23);
            this.lblImageParams.TabIndex = 18;
            // 
            // tbxImageParam3
            // 
            this.tbxImageParam3.Location = new System.Drawing.Point(100, 131);
            this.tbxImageParam3.Name = "tbxImageParam3";
            this.tbxImageParam3.Size = new System.Drawing.Size(185, 20);
            this.tbxImageParam3.TabIndex = 15;
            // 
            // tbxImageParam2
            // 
            this.tbxImageParam2.Location = new System.Drawing.Point(100, 105);
            this.tbxImageParam2.Name = "tbxImageParam2";
            this.tbxImageParam2.Size = new System.Drawing.Size(185, 20);
            this.tbxImageParam2.TabIndex = 14;
            // 
            // tbxImageParam1
            // 
            this.tbxImageParam1.Location = new System.Drawing.Point(100, 79);
            this.tbxImageParam1.Name = "tbxImageParam1";
            this.tbxImageParam1.Size = new System.Drawing.Size(185, 20);
            this.tbxImageParam1.TabIndex = 13;
            // 
            // lblImageRegion
            // 
            this.lblImageRegion.AutoSize = true;
            this.lblImageRegion.Location = new System.Drawing.Point(6, 51);
            this.lblImageRegion.Name = "lblImageRegion";
            this.lblImageRegion.Size = new System.Drawing.Size(56, 13);
            this.lblImageRegion.TabIndex = 6;
            this.lblImageRegion.Text = "Bildregion:";
            // 
            // lblCamera
            // 
            this.lblCamera.AutoSize = true;
            this.lblCamera.Location = new System.Drawing.Point(6, 22);
            this.lblCamera.Name = "lblCamera";
            this.lblCamera.Size = new System.Drawing.Size(46, 13);
            this.lblCamera.TabIndex = 5;
            this.lblCamera.Text = "Kamera:";
            // 
            // cbxCamName
            // 
            this.cbxCamName.FormattingEnabled = true;
            this.cbxCamName.Location = new System.Drawing.Point(100, 19);
            this.cbxCamName.Name = "cbxCamName";
            this.cbxCamName.Size = new System.Drawing.Size(104, 21);
            this.cbxCamName.TabIndex = 4;
            this.cbxCamName.SelectedIndexChanged += new System.EventHandler(this.OnCbxCamIndex_SelectedIndexChanged);
            // 
            // lblImageParam3
            // 
            this.lblImageParam3.Location = new System.Drawing.Point(6, 134);
            this.lblImageParam3.Name = "lblImageParam3";
            this.lblImageParam3.Size = new System.Drawing.Size(86, 23);
            this.lblImageParam3.TabIndex = 3;
            this.lblImageParam3.Text = "lblImageParam3";
            // 
            // lblImageParam2
            // 
            this.lblImageParam2.Location = new System.Drawing.Point(6, 108);
            this.lblImageParam2.Name = "lblImageParam2";
            this.lblImageParam2.Size = new System.Drawing.Size(86, 23);
            this.lblImageParam2.TabIndex = 2;
            this.lblImageParam2.Text = "lblImageParam2";
            // 
            // lblImageParam1
            // 
            this.lblImageParam1.Location = new System.Drawing.Point(6, 82);
            this.lblImageParam1.Name = "lblImageParam1";
            this.lblImageParam1.Size = new System.Drawing.Size(86, 23);
            this.lblImageParam1.TabIndex = 1;
            this.lblImageParam1.Text = "Region-Param.:";
            // 
            // cbxImageRegion
            // 
            this.cbxImageRegion.FormattingEnabled = true;
            this.cbxImageRegion.Location = new System.Drawing.Point(100, 48);
            this.cbxImageRegion.Name = "cbxImageRegion";
            this.cbxImageRegion.Size = new System.Drawing.Size(104, 21);
            this.cbxImageRegion.TabIndex = 0;
            this.cbxImageRegion.SelectedIndexChanged += new System.EventHandler(this.cbxImageRegion_SelectedIndexChanged);
            // 
            // groupBoxOperation
            // 
            this.groupBoxOperation.Controls.Add(this.lblCheckerType);
            this.groupBoxOperation.Controls.Add(this.lblFilterTyp);
            this.groupBoxOperation.Controls.Add(this.cbxCheckerTyp);
            this.groupBoxOperation.Controls.Add(this.cbxFilterTyp);
            this.groupBoxOperation.Controls.Add(this.lblOpParams);
            this.groupBoxOperation.Controls.Add(this.cbxOperator);
            this.groupBoxOperation.Controls.Add(this.lblOpType);
            this.groupBoxOperation.Location = new System.Drawing.Point(17, 217);
            this.groupBoxOperation.Name = "groupBoxOperation";
            this.groupBoxOperation.Size = new System.Drawing.Size(321, 216);
            this.groupBoxOperation.TabIndex = 1;
            this.groupBoxOperation.TabStop = false;
            this.groupBoxOperation.Text = "Operation";
            // 
            // lblCheckerType
            // 
            this.lblCheckerType.AutoSize = true;
            this.lblCheckerType.Location = new System.Drawing.Point(11, 78);
            this.lblCheckerType.Name = "lblCheckerType";
            this.lblCheckerType.Size = new System.Drawing.Size(64, 13);
            this.lblCheckerType.TabIndex = 21;
            this.lblCheckerType.Text = "Checkertyp:";
            // 
            // lblFilterTyp
            // 
            this.lblFilterTyp.AutoSize = true;
            this.lblFilterTyp.Location = new System.Drawing.Point(9, 51);
            this.lblFilterTyp.Name = "lblFilterTyp";
            this.lblFilterTyp.Size = new System.Drawing.Size(46, 13);
            this.lblFilterTyp.TabIndex = 20;
            this.lblFilterTyp.Text = "Filtertyp:";
            // 
            // cbxCheckerTyp
            // 
            this.cbxCheckerTyp.FormattingEnabled = true;
            this.cbxCheckerTyp.Location = new System.Drawing.Point(87, 75);
            this.cbxCheckerTyp.Name = "cbxCheckerTyp";
            this.cbxCheckerTyp.Size = new System.Drawing.Size(104, 21);
            this.cbxCheckerTyp.TabIndex = 19;
            // 
            // cbxFilterTyp
            // 
            this.cbxFilterTyp.FormattingEnabled = true;
            this.cbxFilterTyp.Location = new System.Drawing.Point(87, 48);
            this.cbxFilterTyp.Name = "cbxFilterTyp";
            this.cbxFilterTyp.Size = new System.Drawing.Size(104, 21);
            this.cbxFilterTyp.TabIndex = 18;
            // 
            // lblOpParams
            // 
            this.lblOpParams.BackColor = System.Drawing.SystemColors.ControlDark;
            this.lblOpParams.Location = new System.Drawing.Point(6, 181);
            this.lblOpParams.Name = "lblOpParams";
            this.lblOpParams.Size = new System.Drawing.Size(309, 23);
            this.lblOpParams.TabIndex = 17;
            // 
            // cbxOperator
            // 
            this.cbxOperator.FormattingEnabled = true;
            this.cbxOperator.Location = new System.Drawing.Point(87, 21);
            this.cbxOperator.Name = "cbxOperator";
            this.cbxOperator.Size = new System.Drawing.Size(104, 21);
            this.cbxOperator.TabIndex = 16;
            this.cbxOperator.SelectedIndexChanged += new System.EventHandler(this.OnCbxOperator_SelectedIndexChanged);
            // 
            // lblOpType
            // 
            this.lblOpType.AutoSize = true;
            this.lblOpType.Location = new System.Drawing.Point(6, 24);
            this.lblOpType.Name = "lblOpType";
            this.lblOpType.Size = new System.Drawing.Size(51, 13);
            this.lblOpType.TabIndex = 16;
            this.lblOpType.Text = "Operator:";
            // 
            // groupBoxEvaluation
            // 
            this.groupBoxEvaluation.Controls.Add(this.buttonTest);
            this.groupBoxEvaluation.Controls.Add(this.chkAuswertungAktiv);
            this.groupBoxEvaluation.Location = new System.Drawing.Point(344, 217);
            this.groupBoxEvaluation.Name = "groupBoxEvaluation";
            this.groupBoxEvaluation.Size = new System.Drawing.Size(321, 216);
            this.groupBoxEvaluation.TabIndex = 2;
            this.groupBoxEvaluation.TabStop = false;
            this.groupBoxEvaluation.Text = "Auswertung";
            // 
            // buttonTest
            // 
            this.buttonTest.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonTest.Location = new System.Drawing.Point(240, 14);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(75, 23);
            this.buttonTest.TabIndex = 5;
            this.buttonTest.Text = "Test";
            this.buttonTest.UseVisualStyleBackColor = true;
            this.buttonTest.Click += new System.EventHandler(this.OnButtonTest_Click);
            // 
            // chkAuswertungAktiv
            // 
            this.chkAuswertungAktiv.AutoSize = true;
            this.chkAuswertungAktiv.Location = new System.Drawing.Point(7, 20);
            this.chkAuswertungAktiv.Name = "chkAuswertungAktiv";
            this.chkAuswertungAktiv.Size = new System.Drawing.Size(64, 17);
            this.chkAuswertungAktiv.TabIndex = 0;
            this.chkAuswertungAktiv.Text = "Aktiviert";
            this.chkAuswertungAktiv.ThreeState = true;
            this.chkAuswertungAktiv.UseVisualStyleBackColor = true;
            this.chkAuswertungAktiv.CheckedChanged += new System.EventHandler(this.OnCheckAuswertungAktiv_CheckedChanged);
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(344, 448);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 3;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(263, 448);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Abbruch";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // groupBoxInstruction
            // 
            this.groupBoxInstruction.Controls.Add(this.lblInstrParams);
            this.groupBoxInstruction.Controls.Add(this.tbxDescription);
            this.groupBoxInstruction.Controls.Add(this.tbxName);
            this.groupBoxInstruction.Controls.Add(this.tbxInstructionNumber);
            this.groupBoxInstruction.Controls.Add(this.lblNumber);
            this.groupBoxInstruction.Controls.Add(this.lblDescription);
            this.groupBoxInstruction.Controls.Add(this.lblInstructionName);
            this.groupBoxInstruction.Location = new System.Drawing.Point(17, 18);
            this.groupBoxInstruction.Name = "groupBoxInstruction";
            this.groupBoxInstruction.Size = new System.Drawing.Size(321, 193);
            this.groupBoxInstruction.TabIndex = 2;
            this.groupBoxInstruction.TabStop = false;
            this.groupBoxInstruction.Text = "Instruktion";
            // 
            // tbxDescription
            // 
            this.tbxDescription.Location = new System.Drawing.Point(87, 79);
            this.tbxDescription.Name = "tbxDescription";
            this.tbxDescription.Size = new System.Drawing.Size(228, 20);
            this.tbxDescription.TabIndex = 12;
            // 
            // tbxName
            // 
            this.tbxName.Location = new System.Drawing.Point(87, 48);
            this.tbxName.Name = "tbxName";
            this.tbxName.Size = new System.Drawing.Size(228, 20);
            this.tbxName.TabIndex = 11;
            // 
            // tbxInstructionNumber
            // 
            this.tbxInstructionNumber.Location = new System.Drawing.Point(87, 19);
            this.tbxInstructionNumber.Name = "tbxInstructionNumber";
            this.tbxInstructionNumber.ReadOnly = true;
            this.tbxInstructionNumber.Size = new System.Drawing.Size(33, 20);
            this.tbxInstructionNumber.TabIndex = 10;
            // 
            // lblNumber
            // 
            this.lblNumber.AutoSize = true;
            this.lblNumber.Location = new System.Drawing.Point(6, 22);
            this.lblNumber.Name = "lblNumber";
            this.lblNumber.Size = new System.Drawing.Size(49, 13);
            this.lblNumber.TabIndex = 9;
            this.lblNumber.Text = "Nummer:";
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(6, 82);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(75, 13);
            this.lblDescription.TabIndex = 8;
            this.lblDescription.Text = "Beschreibung:";
            // 
            // lblInstructionName
            // 
            this.lblInstructionName.AutoSize = true;
            this.lblInstructionName.Location = new System.Drawing.Point(9, 51);
            this.lblInstructionName.Name = "lblInstructionName";
            this.lblInstructionName.Size = new System.Drawing.Size(72, 13);
            this.lblInstructionName.TabIndex = 7;
            this.lblInstructionName.Text = "Bezeichnung:";
            // 
            // lblInstrParams
            // 
            this.lblInstrParams.BackColor = System.Drawing.SystemColors.ControlDark;
            this.lblInstrParams.Location = new System.Drawing.Point(6, 157);
            this.lblInstrParams.Name = "lblInstrParams";
            this.lblInstrParams.Size = new System.Drawing.Size(309, 23);
            this.lblInstrParams.TabIndex = 22;
            // 
            // DlgInstruction
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(685, 483);
            this.Controls.Add(this.groupBoxInstruction);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.groupBoxEvaluation);
            this.Controls.Add(this.groupBoxOperation);
            this.Controls.Add(this.groupBoxImage);
            this.Name = "DlgInstruction";
            this.Text = "DlgInstruction";
            this.groupBoxImage.ResumeLayout(false);
            this.groupBoxImage.PerformLayout();
            this.groupBoxOperation.ResumeLayout(false);
            this.groupBoxOperation.PerformLayout();
            this.groupBoxEvaluation.ResumeLayout(false);
            this.groupBoxEvaluation.PerformLayout();
            this.groupBoxInstruction.ResumeLayout(false);
            this.groupBoxInstruction.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxImage;
        private System.Windows.Forms.GroupBox groupBoxOperation;
        private System.Windows.Forms.GroupBox groupBoxEvaluation;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.CheckBox chkAuswertungAktiv;
        private System.Windows.Forms.ComboBox cbxImageRegion;
        private System.Windows.Forms.Label lblImageParam3;
        private System.Windows.Forms.Label lblImageParam2;
        private System.Windows.Forms.Label lblImageParam1;
        private System.Windows.Forms.Label lblImageRegion;
        private System.Windows.Forms.Label lblCamera;
        private System.Windows.Forms.ComboBox cbxCamName;
        private System.Windows.Forms.GroupBox groupBoxInstruction;
        private System.Windows.Forms.TextBox tbxImageParam3;
        private System.Windows.Forms.TextBox tbxImageParam2;
        private System.Windows.Forms.TextBox tbxImageParam1;
        private System.Windows.Forms.TextBox tbxDescription;
        private System.Windows.Forms.TextBox tbxName;
        private System.Windows.Forms.TextBox tbxInstructionNumber;
        private System.Windows.Forms.Label lblNumber;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblInstructionName;
        private System.Windows.Forms.Button buttonTest;
        private System.Windows.Forms.ComboBox cbxOperator;
        private System.Windows.Forms.Label lblOpType;
        private System.Windows.Forms.Label lblImageParams;
        private System.Windows.Forms.Label lblOpParams;
        private System.Windows.Forms.Label lblCheckerType;
        private System.Windows.Forms.Label lblFilterTyp;
        private System.Windows.Forms.ComboBox cbxCheckerTyp;
        private System.Windows.Forms.ComboBox cbxFilterTyp;
        private System.Windows.Forms.Label lblInstrParams;
    }
}