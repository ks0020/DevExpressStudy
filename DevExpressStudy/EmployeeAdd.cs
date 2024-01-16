using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DevExpressStudy
{
    public partial class EmployeeAdd : Form
    {
        public EmployeeAdd()
        {
            InitializeComponent();

            BindDepartmentCodes();
            CodeCombo.SelectedIndexChanged += DepartmentNameDisplay;
            pictureEdit.Click += PictureEdit_Click;

            AddBtn.Click += Add;
            CloseBtn.Click += Close;
        }

        string connectionString = @"Data Source=[서버이름];Initial Catalog=[DB이름];Integrated Security=True";



        // 콤보 박스 부서코드 적용
        private void BindDepartmentCodes()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT 부서코드 FROM dbo.department";
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
                    MessageBox.Show("오류 : " + ex.Message);
                }
            }
        }

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
                    MessageBox.Show("오류 : " + ex.Message);
                }
            }
            return departmentName;
        }

        private void DepartmentNameDisplay(object sender, EventArgs e)
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

        // 아이디 중복 체크
        private bool IdCheck(string userId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM dbo.employee WHERE 로그인ID = @UserId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                try
                {
                    connection.Open();
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("에러 발생: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        // 비밀번호 해싱 함수
        private string HashPassword(string passowrd)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(passowrd));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    sBuilder.Append(bytes[i].ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }

        private string imagePath;
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

        // 저장하기
        public void Add(object sender, EventArgs e)
        {
            // 필수 정보 확인
            if (string.IsNullOrEmpty(CodeCombo.Text) || string.IsNullOrEmpty(EmployeeCodeText.Text) || string.IsNullOrEmpty(EmployeeNameText.Text) || string.IsNullOrEmpty(UserIdText.Text) || string.IsNullOrEmpty(UserPasswordText.Text))
            {
                MessageBox.Show("필수 정보를 입력 해주세요..", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 이메일 주소형식 확인
            // 단, 이메일은 필수가 아니므로 비어있다면 그냥 지나친다.
            if (!string.IsNullOrEmpty(EmailText.Text) && !EmailCheck(EmailText.Text))
            {
                MessageBox.Show("올바른 이메일 주소 형식이 아닙니다.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 비밀번호 해싱
            string hashedPassword = HashPassword(UserPasswordText.Text);

            // 비밀번호 유효성 검사
            if (UserPasswordText.Text.Length < 8 || !Regex.IsMatch(UserPasswordText.Text, @"^(?=.*[a-zA-Z])(?=.*[0-9]).{8,}$"))
            {
                MessageBox.Show("비밀번호는 8자 이상이어야 하며, 영문과 숫자를 혼용하여야 합니다.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 아이디 중복 확인
            if (IdCheck(UserIdText.Text))
            {
                MessageBox.Show("이미 존재하는 아이디입니다.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string destinationPath = "C:\\저장할 경로\\";
                string fileName = Path.GetFileName(imagePath);
                string destinationFilePath = Path.Combine(destinationPath, fileName);

                File.Copy(imagePath, destinationFilePath, true);
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // 이미지 업로드 및 이미지 테이블, 직원 정보 테이블에 추가
                    string query = @"
                INSERT INTO dbo.ImageTable (ImageName, ImageRoute, EmployeeCode) 
                VALUES (@ImageName, @ImageRoute, @ECode);

                INSERT INTO dbo.employee (부서코드, 부서명, 사원코드, 사원명, 로그인ID, 비밀번호, 직위, 고용형태, 휴대전화, 이메일, 메신저ID, 메모) 
                VALUES (@DepartmentCode, @DepartmentName, @EmployeeCode, @EmployeeName, @UserId, @Password, @Position, @Type, @Contact, @Email, @MessengerId, @Memo);
            ";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@ImageName", Path.GetFileName(imagePath)); // ImageName에 이미지 파일의 이름을 설정합니다.
                    command.Parameters.AddWithValue("@ImageRoute", imagePath); // ImageRoute에 이미지 파일의 경로를 설정합니다.
                    command.Parameters.AddWithValue("@ECode", EmployeeCodeText.Text); // EmployeeCode에 직원 코드를 설정합니다.

                    // 나머지 직원 정보 파라미터 추가
                    command.Parameters.AddWithValue("@DepartmentCode", CodeCombo.Text);
                    command.Parameters.AddWithValue("@DepartmentName", DepartmentNameText.Text);
                    command.Parameters.AddWithValue("@EmployeeCode", EmployeeCodeText.Text);
                    command.Parameters.AddWithValue("@EmployeeName", EmployeeNameText.Text);
                    command.Parameters.AddWithValue("@UserId", UserIdText.Text);
                    command.Parameters.AddWithValue("@Password", hashedPassword);
                    command.Parameters.AddWithValue("@Position", PositionText.Text);
                    command.Parameters.AddWithValue("@Type", TypeText.Text);
                    command.Parameters.AddWithValue("@Contact", ContactText.Text);
                    command.Parameters.AddWithValue("@Email", EmailText.Text);
                    command.Parameters.AddWithValue("@MessengerId", MessengerText.Text);
                    command.Parameters.AddWithValue("@Memo", MemoText.Text);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("성공적으로 추가가 되었습니다!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("세부정보를 저장하지 못했습니다.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("에러발생: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Close(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
