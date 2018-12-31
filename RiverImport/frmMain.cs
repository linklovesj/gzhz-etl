using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RiverImport.DataCompilation;

namespace RiverImport
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void showMdiChild(Form form)
        {
            form.MdiParent = this;
            form.Show();
        }

        private Form checkOpened(Type t)
        {
            foreach (Form f in MdiChildren)
            {
                if (f.GetType() == t)
                {
                    return f;
                }
            }

            return null;
        }

        private void btn187_Click(object sender, EventArgs e)
        {
            Form f = checkOpened(typeof(frm187));
            if (f == null)
            {
                frm187 form = new frm187();
                showMdiChild(form);
            }
            else
            {
                f.Focus();
            }
        }

        private void btnWeekReport_Click(object sender, EventArgs e)
        {
            Form f = checkOpened(typeof(frmWeek));
            if (f == null)
            {
                frmWeek form = new frmWeek();
                showMdiChild(form);
            }
            else
            {
                f.Focus();
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            Form f = checkOpened(typeof(frmWeek));
            if (f == null)
            {
                FormTimer form = new FormTimer();
                showMdiChild(form);
            }
            else
            {
                f.Focus();
            }
        }

        private void tsbWeekAuto_Click(object sender, EventArgs e)
        {
            Form f = checkOpened(typeof(frmWeek));
            if (f == null)
            {
                FormTimer form = new FormTimer();
                showMdiChild(form);
            }
            else
            {
                f.Focus();
            }
        }

        private void tsbDataDeal_Click(object sender, EventArgs e)
        {
            Form f = checkOpened(typeof(Form3));
            if (f == null)
            {
                Form3 form = new Form3();
                showMdiChild(form);
            }
            else
            {
                f.Focus();
            }
        }

        private void tsbDataCompilation_Click(object sender, EventArgs e)
        {
            Form f = checkOpened(typeof(frmDataCompilation));
            if (f == null)
            {
                frmDataCompilation form = new frmDataCompilation();
                showMdiChild(form);
            }
            else
            {
                f.Focus();
            }
        }

        private void tsbDateEtl_Click(object sender, EventArgs e)
        {
            Form f = checkOpened(typeof(frmDataEtl));
            if (f == null)
            {
                frmDataEtl form = new frmDataEtl();
                showMdiChild(form);
            }
            else
            {
                f.Focus();
            }
        }

        private void tsbRiverPlotting_Click(object sender, EventArgs e)
        {
            Form f = checkOpened(typeof(PlottingDataFrm));
            if (f == null)
            {
                PlottingDataFrm form = new PlottingDataFrm();
                showMdiChild(form);
            }
            else
            {
                f.Focus();
            }
        }

        private void tsbImportSection_Click(object sender, EventArgs e)
        {
            Form f = checkOpened(typeof(Form1));
            if (f == null)
            {
                Form1 form = new Form1();
                showMdiChild(form);
            }
            else
            {
                f.Focus();
            }
        }

        private void tsbImportOnePlatform_Click(object sender, EventArgs e)
        {
            Form f = checkOpened(typeof(frmImportOnePlatform));
            if (f == null)
            {
                frmImportOnePlatform form = new frmImportOnePlatform();
                showMdiChild(form);
            }
            else
            {
                f.Focus();
            }
        }

        private void tsbProvinceData_Click(object sender, EventArgs e)
        {
            Form f = checkOpened(typeof(ProvinceDataFrm));
            if (f == null)
            {
                ProvinceDataFrm form = new ProvinceDataFrm();
                showMdiChild(form);
            }
            else
            {
                f.Focus();
            }
        }
    }
}
