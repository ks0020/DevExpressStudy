using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
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
            modifyBtn.Click += EmployeeModify;

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
            SetReadOnlyGridView();
        }

        
        private void SetReadOnlyGridView()
        {
            GridView gridView = EmployeeTable.MainView as GridView;

            if (gridView != null)
            {
                foreach (GridColumn column in gridView.Columns)
                {
                    // 각 열에 대한 읽기 전용 설정
                    column.OptionsColumn.ReadOnly = true;

                    // 각 열에 대한 RepositoryItem 수정
                    RepositoryItemTextEdit repositoryItem = column.ColumnEdit as RepositoryItemTextEdit;
                    if (repositoryItem != null)
                    {
                        repositoryItem.ReadOnly = true;
                        repositoryItem.Appearance.Options.UseTextOptions = true;
                        repositoryItem.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near; // 필요에 따라 정렬 방식도 설정
                    }
                }
            }
        }

        private void EmployeeAdd(object sender, EventArgs e)
        {
            EmployeeAdd nAdd = new EmployeeAdd();
            nAdd.Show();
        }

        private void EmployeeModify(object sender, EventArgs e)
        {
            int selectCell = (EmployeeTable.FocusedView as GridView).FocusedRowHandle;
            Console.WriteLine(selectCell);
            if (selectCell >= 0)
            {
                DataRow selectedRow = (EmployeeTable.FocusedView as GridView).GetDataRow(selectCell);
                Console.WriteLine(selectedRow);

                // 선택된 행의 데이터를 EmployeeInfo 폼으로 전달
                EmployeeModify eModify = new EmployeeModify(selectedRow);
                eModify.ShowDialog();
            }
        }

        private void Close(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
    