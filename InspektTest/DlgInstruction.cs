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
using static Visutronik.Imaging.Checker;

namespace Visutronik
{
    public partial class DlgInstruction : Form
    {
        public Instruction GetInstruction() { return _instruction; }

        private Rectangle _rect;
        private readonly Instruction _instruction;

        public DlgInstruction(Rectangle rect, int cam = 0)
        {
            InitializeComponent();

            _instruction = new Instruction();
            _rect = rect;
            _instruction.Operation = "Checker";
            _instruction.OperatorIdx = (int)OperatorType.Checker;
            _instruction.FilterIdx = -1;
            _instruction.CheckerIdx = -1;
            _instruction.CameraIndex = cam;
            _instruction.ImageAreaIndex = (int)ImageAreaType.Rect;

            InitControls();
            SetValuesToControls();
        }

        public DlgInstruction(Instruction inst)
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

            cbxCamIndex.Items.AddRange(InstructionHelper.CameraNames);
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

            cbxCamIndex.SelectedIndex = _instruction.CameraIndex;
            cbxImageRegion.SelectedIndex = _instruction.ImageAreaIndex;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonOk_Click(object sender, EventArgs e)
        {
            // TODO Prüfen der Eingaben
            _instruction.CameraIndex = cbxCamIndex.SelectedIndex - 1;
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

        private void cbxOperator_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        private void OnCheckAuswertungAktiv_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAuswertungAktiv.CheckState == CheckState.Indeterminate)
            {

            }
        }

        private void cbxCamIndex_SelectedIndexChanged(object sender, EventArgs e)
        {

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

        private void buttonTest_Click(object sender, EventArgs e)
        {

        }

    }
}
