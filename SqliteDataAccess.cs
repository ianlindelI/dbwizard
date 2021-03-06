﻿using Dapper;
using DBWizard.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;

namespace DBWizard
{
    class SqliteDataAccess
    {
        public List<Student> FindStudentByLastname(string last_name)
        {
            //A using statement protects us as a failsafe: it guarantees the connection to the
            //database will be closed, even in event of an application or computer crash.
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                string strQuery = "SELECT * FROM students WHERE LastName=@LastName";

                var output = cnn.Query<Student>(strQuery, new { LastName = last_name });
                return output.ToList();
            }
        }

        //Can also be used for search function.
        public List<Student> FindStudentByStudentId(string studentid_in)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                string strQuery = "SELECT * FROM students WHERE student_id=@studentid";

                var output = cnn.Query<Student>(strQuery, new { studentid = studentid_in });
                return output.ToList();
            }
        }

        internal void UpdateStudent(Student stu)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                string strQuery = "UPDATE Students SET student_id = @student_id, FirstName = @First_Name, LastName = @Last_Name," +
                "DOB = @_dob, Gender = @_gender, Address = @_address, GradeLevel = @Grade_Level, Photo = @_photo" +
                "WHERE id = @_id;";

                cnn.Execute(strQuery, new
                {
                    _id = stu.id,
                    student_id = stu.student_id,
                    First_Name = stu.FirstName,
                    Last_Name = stu.LastName,
                    _dob = stu.DOB,
                    _gender = stu.gender,
                    _address = stu.address,
                    Grade_Level = stu.GradeLevel,
                    _photo = stu.photo ?? null
                });
            }
        }

        internal void InsertNewStudent(Student stu)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                string strQuery = "INSERT INTO STUDENTS (student_id, parent_id, FirstName, LastName, DOB, gender, address, program_id, school_id, GradeLevel, photo)" +
                    "VALUES (@student_id, @parent_id, @FirstName, @LastName, @DOB, @gender, @address, @program_id, @school_id, @GradeLevel, @photo)";
                cnn.Execute(strQuery, stu);
            }
        }

        /* Inserts a new parent.
         * returns parent id as the new row # it just inserted.
         */
        internal int InsertNewParent(Parent par)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                string strQuery = "INSERT INTO PARENTS (FirstName,LastName,PhoneNumber,EmailAddress)" +
                    "VALUES (@FirstName,@LastName,@PhoneNumber,@EmailAddress);";
                var affectedRow = cnn.Execute(strQuery, par);
                return affectedRow;
            }
        }

        internal void UpdateParent(Parent par, int parent_id_in)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                string strQuery = "UPDATE Parents SET FirstName = @First_Name, LastName = @Last_Name," +
                    "PhoneNumber = @Phone_Number, EmailAddress = @Email_Address" +
                    "WHERE parent_id = @_id;";

                cnn.Execute(strQuery, new
                {
                    _id = parent_id_in,
                    First_Name = par.FirstName,
                    Last_Name = par.LastName,
                    Phone_Number = par.PhoneNumber,
                    Email_Address = par.EmailAddress
                });
            }
        }

        internal List<Program> GetPrograms()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                string strQuery = "SELECT name FROM programs;";

                var output = cnn.Query<Program>(strQuery, new DynamicParameters());
                return output.ToList();
            }
        }

        internal List<School> GetSchools()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                string strQuery = "SELECT name FROM schools;";

                var output = cnn.Query<School>(strQuery, new DynamicParameters());
                return output.ToList();
            }
        }

        public List<Parent> GetParentByID(int parent_id)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                string strQuery = "SELECT * FROM parents WHERE id=@id;";

                var output = cnn.Query<Parent>(strQuery, new { id = parent_id });

                return output.ToList();
            }
        }

        private static string LoadConnectionString(string id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }

    } //class SqliteDataAccess
}
