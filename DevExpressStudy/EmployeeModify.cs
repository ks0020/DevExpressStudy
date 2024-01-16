using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DevExpressStudy
{
    public partial class EmployeeModify : Form
    {
        readonly string connectionString = @"Data Source=[서버이름];Initial Catalog=[DB이름];Integrated Security=True";
        public EmployeeModify(DataRow employeeData)
        {
            InitializeComponent();

            this.employeeData = employeeData;

            CodeCombo.Text = employeeData["부서코드"].ToString();
            DepartmentNameText.Text = employeeData["부서명"].ToString();
            EmployeeCodeText.Text = employeeData["사원코드"].ToString();
            EmployeeNameText.Text = employeeData["사원명"].ToString();
            PositionText.Text = employeeData["직위"].ToString();
            TypeText.Text = employeeData["고용형태"].ToString();
            ContactText.Text = employeeData["휴대전화"].ToString();
            EmailText.Text = employeeData["이메일"].ToString();
            MessengerText.Text = employeeData["메신저ID"].ToString();
            MemoText.Text = employeeData["메모"].ToString();

            LoadEmployeeImage(employeeData["사원코드"].ToString());

            pictureEdit2.Click += PictureEdit_Click;
            ModifyBtn.Click += Modify;
            CloseBtn.Click += Close;

            BindDepartmentCodes();
            CodeCombo.SelectedIndexChanged += DepartmentNameSelect;
        }
        private readonly DataRow employeeData;
        private string imagePath;

        // 사진 불러오기
        private void LoadEmployeeImage(string employeeCode)
        {
            try
            {
                if (this.employeeData != null)
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string query = "SELECT ImageRoute FROM dbo.ImageTable WHERE EmployeeCode = @EmployeeCode";
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@EmployeeCode", employeeCode);

                        object result = command.ExecuteScalar();

                        if (result != null)
                        {
                            string imagePath = result.ToString();

                            // 이미지를 ImageEdit 컨트롤에 로드
                            pictureEdit2.Image = Image.FromFile(imagePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("이미지 불러오기 실패: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        // 픽쳐에딧 클릭시 다이얼로그
        private void PictureEdit_Click(object sender, EventArgs e)
        {
            // 파일 선택 다이얼로그를 통해 이미지 로드
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.jpg; *.jpeg; *.png; *.gif)|*.jpg; *.jpeg; *.png; *.gif";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                imagePath = openFileDialog.FileName;

                // 선택한 이미지 파일을 PictureEdit 컨트롤에 로드
                (sender as PictureEdit).Image = Image.FromFile(imagePath);
            }
        }

        // 콤보 박스에 부서코드 적용
        private void BindDepartmentCodes()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT 부서코드 FROM department";
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string departmentCode = reader["부서코드"].ToString();
                        CodeCombo.Items.Add(departmentCode);
                    }
                    
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("오류: " + ex.Message);
                }
            }
        }

        // 선택된 부서 코드로 해당 부서명 가져오기
        private string GetDepartmentName(string selectedCode)
        {
            string departmentName = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT 부서명 FROM department WHERE 부서코드 = @DepartmentCode";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@DepartmentCode", selectedCode);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        departmentName = result.ToString();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("오류: " + ex.Message);
                }
            }

            return departmentName;
        }

        // 부서 코드 콤보박스 선택 변경 이벤트 핸들러
        private void DepartmentNameSelect(object sender, EventArgs e)
        {
            string selectedCode = CodeCombo.SelectedItem.ToString();
            string departmentName = GetDepartmentName(selectedCode);
            DepartmentNameText.Text = departmentName;
        }

        // 이메일 형식이 맞는지 체크
        private bool EmailCheck(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void Modify(object sender, EventArgs e)
        {
            // 이메일 주소형식 확인
            // 단, 이메일은 필수가 아니므로 비어있다면 그냥 지나친다.
            if (!string.IsNullOrEmpty(EmailText.Text) && !EmailCheck(EmailText.Text))
            {
                MessageBox.Show("올바른 이메일 주소 형식이 아닙니다.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string path = "C:\\저장할 경로\\";
                    string fileName = Path.GetFileName(imagePath);
                    string filePath = Path.Combine(path, fileName);

                    // 중복된 파일명 체크 및 처리
                    if (File.Exists(filePath))
                    {
                        // 파일이 이미 존재할 경우 중복을 피하기 위해 괄호와 숫자 추가
                        string extension = Path.GetExtension(fileName);
                        string justName = Path.GetFileNameWithoutExtension(fileName);

                        int count = 1;
                        do
                        {
                            fileName = $"{justName}({count}){extension}";
                            filePath = Path.Combine(path, fileName);
                            count++;
                        } while (File.Exists(filePath));
                    }

                    File.Copy(imagePath, filePath, true);

                    connection.Open();

                    string checkImageQuery = "SELECT COUNT(*) FROM dbo.ImageTable WHERE EmployeeCode = @ECode";
                    SqlCommand checkImageCommand = new SqlCommand(checkImageQuery, connection);
                    checkImageCommand.Parameters.AddWithValue("@ECode", EmployeeCodeText.Text);

                    int imageCount = (int)checkImageCommand.ExecuteScalar();

                    if (imageCount > 0)
                    {
                        // 이미지 업데이트
                        string updateImageQuery = "UPDATE dbo.ImageTable SET ";
                        if (!string.IsNullOrEmpty(imagePath))
                        {
                            updateImageQuery += "ImageName = @ImageName, ImageRoute = @ImageRoute ";
                        }
                        updateImageQuery += "WHERE EmployeeCode = @ECode";

                        SqlCommand updateImageCommand = new SqlCommand(updateImageQuery, connection);
                        if (!string.IsNullOrEmpty(imagePath))
                        {
                            updateImageCommand.Parameters.AddWithValue("@ImageName", Path.GetFileName(imagePath));
                            updateImageCommand.Parameters.AddWithValue("@ImageRoute", imagePath);
                            updateImageCommand.Parameters.AddWithValue("@ECode", EmployeeCodeText.Text);

                            try
                            {
                                updateImageCommand.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("이미지 업데이트 중 에러 발생: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    else
                    {
                        string insertImageQuery = "INSERT INTO dbo.ImageTable (EmployeeCode, ImageName, ImageRoute) VALUES (@ECode, @ImageName, @ImageRoute)";
                        SqlCommand insertImageCommand = new SqlCommand(insertImageQuery, connection);
                        insertImageCommand.Parameters.AddWithValue("@ECode", EmployeeCodeText.Text);
                        insertImageCommand.Parameters.AddWithValue("@ImageName", Path.GetFileName(imagePath));
                        insertImageCommand.Parameters.AddWithValue("@ImageRoute", imagePath);
                    }

                    // 직원 정보 테이블에 업데이트
                    string updateEmployeeQuery = @"
                UPDATE dbo.employee 
                SET 부서코드 = @DepartmentCode, 부서명 = @DepartmentName, 
                    사원코드 = @EmployeeCode, 사원명 = @EmployeeName,
                    직위 = @Position, 고용형태 = @Type, 
                    휴대전화 = @Contact, 이메일 = @Email, 
                    메신저ID = @MessengerId, 메모 = @Memo
                WHERE 사원코드 = @EmployeeCode
            ";

                    SqlCommand updateEmployeeCommand = new SqlCommand(updateEmployeeQuery, connection);
                    updateEmployeeCommand.Parameters.AddWithValue("@DepartmentCode", CodeCombo.Text);
                    updateEmployeeCommand.Parameters.AddWithValue("@DepartmentName", DepartmentNameText.Text);
                    updateEmployeeCommand.Parameters.AddWithValue("@EmployeeCode", EmployeeCodeText.Text);
                    updateEmployeeCommand.Parameters.AddWithValue("@EmployeeName", EmployeeNameText.Text);
                    updateEmployeeCommand.Parameters.AddWithValue("@Position", PositionText.Text);
                    updateEmployeeCommand.Parameters.AddWithValue("@Type", TypeText.Text);
                    updateEmployeeCommand.Parameters.AddWithValue("@Contact", ContactText.Text);
                    updateEmployeeCommand.Parameters.AddWithValue("@Email", EmailText.Text);
                    updateEmployeeCommand.Parameters.AddWithValue("@MessengerId", MessengerText.Text);
                    updateEmployeeCommand.Parameters.AddWithValue("@Memo", MemoText.Text);

                    int rowsAffected = updateEmployeeCommand.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("성공적으로 수정되었습니다!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("세부정보를 수정하지 못했습니다.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("에러발생: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine(ex.Message);
            }
        }


        private void Close(object sender, EventArgs e)
        {
            // 폼 닫기
            this.Close();
        }
    }
}
