﻿/*
 * Projekt  : Inspekt
 * Datei    : DlgInstruction - Dialog-Klasse zur Erzeugung und Generierung von BV-Anweisungen
 */

// 16.10.2023   add TODOs to find out what to do ;-)
// 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visutronik.Inspektion;
using System.Windows.Forms;
using System.Diagnostics;
using Visutronik.Commons;
//using static Visutronik.Imaging.Checker;

namespace Visutronik
{
    public partial class DlgInstruction : Form
    {
        public InstructionParams MyInstruction
        {
            get { return _instruction; }
            set { _instruction = value; }
        }

        public InstructionParams GetInstruction() { return _instruction; }

        private Rectangle _rect;

        private InstructionParams _instruction;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="cam"></param>
        public DlgInstruction(Rectangle rect, int cam = 0)
        {
            InitializeComponent();

            _instruction = new InstructionParams();
            _rect = rect;
            _instruction.Number = 0;
            _instruction.Operation = "Checker";
            _instruction.OperatorIdx = (int)OperatorType.Checker;
            _instruction.FilterIdx = -1;
            _instruction.CheckerIdx = -1;
            _instruction.CameraIndex = cam;
            _instruction.ImageAreaIndex = (int)ImageAreaType.Rect;
            _instruction.ImageAreaParams = rect.ToString();

            InitControls();
            SetValuesToControls();
        }

        public DlgInstruction(InstructionParams inst)
        {
            InitializeComponent();
            _instruction = inst;

            InitControls();
            SetValuesToControls();
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitControls()
        {
            // None = -1, LoadImage, Filter, Checker,

            cbxOperator.Items.AddRange(InstructionHelper.Operators);
            //new string[] { "Bild laden", "Filter", "Checker" });
            cbxFilterTyp.Items.AddRange(InstructionHelper.FilterTypes);
            cbxCheckerTyp.Items.AddRange(InstructionHelper.CheckerTypes);

            cbxCamName.Items.AddRange(InstructionHelper.CameraNames);
            cbxImageRegion.Items.AddRange(InstructionHelper.AreaNames);

            GuiTools.SetCueText(tbxDescription, "Geben Sie eine kurze Beschreibung ein.");
        }

        private void SetValuesToControls()
        {
            tbxInstructionNumber.Text = _instruction.Number.ToString();
            tbxName.Text = _instruction.Name;
            tbxDescription.Text = _instruction.Description;

            cbxOperator.SelectedIndex = _instruction.OperatorIdx;
            cbxFilterTyp.SelectedIndex = _instruction.FilterIdx;
            cbxCheckerTyp.SelectedIndex = _instruction.CheckerIdx;

            cbxCamName.SelectedIndex = _instruction.CameraIndex;
            cbxImageRegion.SelectedIndex = _instruction.ImageAreaIndex;

            tbxImageParam1.Text = _instruction.ImageAreaParams;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonOk_Click(object sender, EventArgs e)
        {
            bool result = true;

            if (!_instruction.CheckOperationParameters())
            {
                lblOpParams.Text = "Ungültige Werte!";
                result = false;
            }
            else
                lblOpParams.Text = "";

            if (!_instruction.CheckImageAreaParameters())
            {
                lblImageParams.Text = "Ungültige Werte!";
                result = false;
            }
            else
                lblImageParams.Text = "";

            // TODO Prüfen weiterer Eingaben

            if (result)
            {
                _instruction.CameraIndex = cbxCamName.SelectedIndex - 1;
                _instruction.ImageAreaIndex = cbxImageRegion.SelectedIndex;
                _instruction.ImageAreaParams = _rect.ToString();
                _instruction.Name = tbxName.Text != string.Empty ? tbxName.Text : "unbenannt";
                _instruction.Description = tbxDescription.Text;

                // hier 
                _instruction.Operation = "Checker";
                _instruction.OperatorIdx = cbxOperator.SelectedIndex;

                DialogResult = DialogResult.OK;
                Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void OnCbxOperator_SelectedIndexChanged(object sender, EventArgs e)
        {
            // - TODO OnCbxOperator_SelectedIndexChanged
        }


        private void OnCheckAuswertungAktiv_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAuswertungAktiv.CheckState == CheckState.Indeterminate)
            {
                // - TODO OnCheckAuswertungAktiv_CheckedChanged
            }
        }

        private void OnCbxCamIndex_SelectedIndexChanged(object sender, EventArgs e)
        {
            // - TODO OnCbxCamIndex_SelectedIndexChanged
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxImageRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cbx = sender as ComboBox;
            int idx = cbx.SelectedIndex;

            _instruction.ImageAreaIndex = idx;
            switch (idx)
            {
                case (int)ImageAreaType.Full:
                    lblImageParam1.Text = "";
                    lblImageParam2.Text = "";
                    lblImageParam3.Text = "";
                    break;
                case (int)ImageAreaType.Rect:
                    lblImageParam1.Text = "Ecke links oben";
                    lblImageParam2.Text = "Breite";
                    lblImageParam3.Text = "Höhe";
                    break;
                case (int)ImageAreaType.Circle:
                    lblImageParam1.Text = "Mittelpunkt";
                    lblImageParam2.Text = "Radius";
                    lblImageParam3.Text = "";
                    break;
                    //TODO add more ImageAreaType types
            }
        }

        private void GroupBoxImage_Enter(object sender, EventArgs e)
        {
            Debug.WriteLine("GroupBoxImage_Enter");
        }

        private void OnButtonTest_Click(object sender, EventArgs e)
        {
            // TODO  DlgInstructions.OnButtonTest_Click
        }

    }
}
