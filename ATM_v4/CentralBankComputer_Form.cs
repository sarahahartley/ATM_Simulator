using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ATM_v4
{
    public partial class CentralBank_Form : Form
    {


        public CentralBank_Form()
        {
            InitializeComponent();
        }

        private void RaceConBtn_Click(object sender, EventArgs e)
        {
            ATM.raceCondition = true;
            Close();
        }

        private void NonRaceConBtn_Click(object sender, EventArgs e)
        {
            ATM.raceCondition = false;
            Close();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            ATM.numOfATMs = 1;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            ATM.numOfATMs = 2;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            ATM.numOfATMs = 3;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            ATM.numOfATMs = 4;
        }
    }
}
