using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DevExpressStudy
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            addBtn.Click += EmployeeAdd;


            CloseBtn.Click += Close;

            
            this.Load += MainLoad;
        }

        readonly string connectionString = @"Data Source=DESKTOP-80CKK65;Initial Catalog=Project001;Integrated Security=True";
        private void MainLoad(object sender, EventArgs e)
        {

            SqlDataAdapter dataAdapter;
            DataSet dataSet;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "SELECT * FROM dbo.employee"; 
                    dataAdapter = new SqlDataAdapter(query, connection);
                    dataSet = new DataSet();
                    dataAdapter.Fill(dataSet);

                    // GridControl 및 GridView를 초기화한다.
                    // DevExpress에서 제공하는 데이터 그리드 컨트롤
                    GridControl gridControl = new GridControl();
                    GridView gridView = new GridView(gridControl);


                    DataTable employeeTable = new DataTable();
                    gridControl.DataSource = employeeTable;

                    // GridView의 컬럼 자동 생성을 위한 설정
                    gridView1.PopulateColumns();

                    // GridView를 폼에 추가
                    this.Controls.Add(gridControl);

                    EmployeeTable.DataSource = dataSet.Tables[0]; // 데이터 그리드 뷰에 데이터 바인딩 

                }
                catch (Exception ex)
                {
                    MessageBox.Show("연결 실패: " + ex.Message);
                }
            }
        }

        private void EmployeeAdd(object sender, EventArgs e)
        {
            EmployeeAdd nAdd = new EmployeeAdd();
            nAdd.Show();
        }

        /*private void Modify(object sender, EventArgs e)
        {

        }*/

        private void Close(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
    