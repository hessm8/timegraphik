using System;
using System.Collections.Generic;

namespace Timegraphik.Data {
    public class ScheduleData {
        public ScheduleData() {
            groups = new List<string>();
            subjects = new List<string>();
            teachers = new List<string>();
            rooms = new List<string>();
        }
        public List<string> this[int i] {
            get {
                switch (i) {
                    case 0: return groups;
                    case 1: return subjects;
                    case 2: return teachers;
                    case 3: return rooms;
                    default: throw new Exception("Invalid data");
                }
            }
        }
        public List<string> this[string i] {
            get {
                switch (i) {
                    case "groups":
                    case "Группа": return groups;
                    case "subjects":
                    case "Дисциплина": return subjects;
                    case "teachers":
                    case "Преподаватель": return teachers;
                    case "rooms":
                    case "Аудитория": return rooms;
                    default: throw new Exception("Invalid data");
                }
            }
        }

        public List<string> groups, subjects, teachers, rooms;
    }
}
