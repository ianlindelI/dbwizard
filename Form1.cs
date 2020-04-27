using DBWizard.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

/* References 
 * https://www.youtube.com/watch?v=ayp3tHEkRc0
 * https://www.coderslexicon.com/passing-data-between-forms-using-delegates-and-events/
 * https://dapper-tutorial.net/parameter-anonymous
 * https://www.sqlitetutorial.net/sqlite-foreign-key/
 * https://stackoverflow.com/questions/289680/difference-between-2-dates-in-sqlite
 * https://dzone.com/articles/convert-object-byte-array-and
  */
// Using a delegate to trigger method on the customer view form
// https://www.coderslexicon.com/passing-data-between-forms-using-delegates-and-events/



namespace DBWizard
{
    public partial class Form1 : Form
    {
        SqliteDataAccess SqliteDataAccess = new SqliteDataAccess();
        bool bStudentRecordExists = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FillComboBoxes();
        }

        /* Fills program and school combo boxes on the form
         * with the static data from the sqlite db.
         * input: no input
         * output: void
         */
        private void FillComboBoxes()
        {
            List<Program> programs = new List<Program>();
            programs = SqliteDataAccess.GetPrograms();

            foreach (Program p in programs)
            {
                programComboBox.Items.Add(p.name);
            }

            List<School> schools = new List<School>();
            schools = SqliteDataAccess.GetSchools();
            foreach (School s in schools)
            {
                schoolComboBox.Items.Add(s.name);
            }
        }

        //TODO: implement
        private void student_pictureBox_DoubleClick(object sender, EventArgs e)
        {
            DialogResult dr = pic_openFileDialog.ShowDialog();
            if (dr == DialogResult.OK)
            {
            }
        }

        private void pic_openFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            byte[] dickpic;

            //Chunk file into bytes.
            //If file > 2147483647 bytes long, reject file.
            dickpic = ObjectToByteArray(student_pictureBox.Image);
            if (dickpic.Length >= 2147483647) //THAT'S WHAT SHE SAID
            {
                MessageBox.Show("The selected student photo size is too large. Choose a smaller photo size or shrink the photo.");
                return;
            }
        }

        //TODO: implement
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            DialogResult dr = pic_openFileDialog.ShowDialog();
            if (dr == DialogResult.OK)
            {
                //Place in dicpic box

                //change tag of pic box to something else
                student_pictureBox.Tag = "dickpic";
            }
        }

        private void save_button_Click(object sender, EventArgs e)
        {
            byte[] dickpic;

            //Perform sanity check, ensure all data is filled except picture

            // Check to see if any control is in error.
            foreach (Control c in errorProvider1.ContainerControl.Controls)
            {
                if (errorProvider1.GetError(c) != "")
                {
                    MessageBox.Show("Please fix the errors on the form!");
                    return;
                }
            }

            //Validate the picture is under 2147483647 characters in length.
            dickpic = ObjectToByteArray(student_pictureBox.Image);
            if (dickpic.Length >= 2147483647) //THAT'S WHAT SHE SAID
            {
                MessageBox.Show("The selected student photo size is too large. Choose a smaller photo size or shrink the photo.");
                return;
            }

            //Collect form data in student object
            Student student = new Student();
            student.student_id = student_idTextBox.Text;
            student.FirstName = firstNameTextBox.Text;
            student.LastName = lastNameTextBox.Text;
            student.DOB = dob_dateTimePicker.Value.ToString();
            student.gender = genderComboBox.Text;
            student.address = addressTextBox.Text;
            student.program_id = programComboBox.SelectedIndex + 1;
            student.school_id = schoolComboBox.SelectedIndex + 1;
            student.GradeLevel = gradeLevelComboBox.Text;

            if (student_pictureBox.Tag.ToString() == "DefaultImage")
                student.photo = null;
            else
                student.photo = ObjectToByteArray(student_pictureBox.Image);

            //collect parent
            Parent parent = new Parent();
            parent.LastName = parent_lastNameTextBox.Text;
            parent.FirstName = parent_firstNameTextBox.Text;
            parent.PhoneNumber = long.Parse(phoneNumberNumericUpDown.Value.ToString());
            parent.EmailAddress = emailAddressTextBox.Text;

            //save data to sqldb.
            //update existing student.
            if (SqliteDataAccess.FindStudentByStudentId(student.student_id).Count > 0)
            {
                SqliteDataAccess.UpdateStudent(student);
                SqliteDataAccess.UpdateParent(parent, student.parent_id);
            }
            else //new student
            {
                student.parent_id = SqliteDataAccess.InsertNewParent(parent);
                SqliteDataAccess.InsertNewStudent(student);
                MessageBox.Show("Student successfully added");
            }
        }

        //Convert student photo to byte array for saving to db.
        //Also used to see if the photo is too large for the db blob type.
        private byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);

            return ms.ToArray();
        }

        //Convert student photo from byte array to object.
        // Convert a byte array to an Object
        private Object ByteArrayToObject(byte[] arrBytes)

        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();

            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);

            return obj;
        }

        // Basic input validation on a string given a textbox control
        // ensures only values a-z, with length two or greater.
        private void String_TextBox_Validating(object sender, CancelEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (!Regex.IsMatch(tb.Text, "[A-Za-z]{2,}"))
                errorProvider1.SetError(tb, "Enter a value a-z only of length two or longer.");
            else
                errorProvider1.SetError(tb, "");
        }

        private void ComboBox_Validating(object sender, CancelEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;

            if (cb.SelectedIndex < 0)
            { errorProvider1.SetError(cb, "Select an item."); }
            else
            { errorProvider1.SetError(cb, ""); }
        }

        /* Does not really check to see if it is a valid phone number
         * since the phone companies introduce new area codes all the damn time.
         * Only checks to see the most likely case that the user of the program
         * has left the phone number value as the default minimum value.
         */
        private void phoneNumberNumericUpDown_Validating(object sender, CancelEventArgs e)
        {
            if (phoneNumberNumericUpDown.Value == phoneNumberNumericUpDown.Minimum
            || phoneNumberNumericUpDown.Value == phoneNumberNumericUpDown.Maximum)
            { errorProvider1.SetError(phoneNumberNumericUpDown, "Type in a valid phone number"); }
            else
            { errorProvider1.SetError(phoneNumberNumericUpDown, ""); }
        }

        private void emailAddressTextBox_Validating(object sender, CancelEventArgs e)
        {
            //email address regex
            //^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,5}$
            if (!Regex.IsMatch(emailAddressTextBox.Text, @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,5}$"))
                errorProvider1.SetError(emailAddressTextBox, "Enter a valid email address: user@domain.com");
            else
                errorProvider1.SetError(emailAddressTextBox, "");
        }

        private void dob_dateTimePicker_Validating(object sender, CancelEventArgs e)
        {
            if (dob_dateTimePicker.Value == DateTime.Now)
            { errorProvider1.SetError(dob_dateTimePicker, "Please choose DOB."); }
            else
            { errorProvider1.SetError(dob_dateTimePicker, ""); }
        }

        private void student_idTextBox_Validating(object sender, CancelEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (!Regex.IsMatch(tb.Text, "[A-Za-z0-9]{2,}"))
                errorProvider1.SetError(tb, "Enter a value a-z0-9 only of length two or longer.");
            else
                errorProvider1.SetError(tb, "");

        }

        //TODO: Remove Chaos Monkey button
        private void btnChaosMonkey_Click(object sender, EventArgs e)
        {
            MessageBox.Show("CHAOS MONKEY");
            return;

        }

        private void clear_button_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        // clears the form
        private void ClearForm()
        {
            foreach (Control c in this.Controls)
            {
                if (c is TextBox)
                {
                    TextBox tb = (TextBox)c;
                    tb.Text = null;
                }

                if (c is ComboBox)
                {
                    ComboBox cb = (ComboBox)c;
                    cb.SelectedIndex = -1;
                }

                if (c is DateTimePicker)
                {
                    DateTimePicker dtp = (DateTimePicker)c;
                    dtp.Value = DateTime.Today;
                }

                if (c is NumericUpDown)
                {
                    NumericUpDown nud = (NumericUpDown)c;
                    nud.Value = nud.Minimum;
                }
            }
        }

        private void search_textBox_KeyDown(object sender, KeyEventArgs e)
        {
            List<Student> foundStudents = new List<Student>();

            if (e.KeyCode == Keys.Enter)
            {
                foundStudents_comboBox.Items.Clear();
                foundStudents_comboBox.Text = String.Empty;

                foundStudents = SqliteDataAccess.FindStudentByLastname(search_textBox.Text);

                foreach (Student s in foundStudents)
                {
                    foundStudents_comboBox.Items.Add(s);
                }

                foundStudents_comboBox.Text = (foundStudents.Count > 0) ? "Students found, click here" : "No students found";
            }
        }

        //Do nothing if the user tries to type in the found student combobox.
        private void foundStudents_comboBox_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
        }

        //TODO:Populate the form based on the found student.
        private void foundStudents_comboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            ClearForm();
        }

    }
}
