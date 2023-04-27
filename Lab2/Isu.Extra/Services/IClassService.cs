using System.Collections.ObjectModel;

using Isu.Extra.Entities;
using Isu.Extra.Models;
using Isu.Models;

namespace Isu.Extra.Services;

public interface IClassService
{
    public AcademicClass AssignClass(
        Schedule schedule,
        Discipline discipline,
        StudyUnit studyUnit,
        string weekFlag,
        string dayOfWeek,
        string time,
        Tutor tutor,
        int classroom);

    public void AddStudyUnitToClass(
        Schedule schedule,
        AcademicClass academicClass,
        StudyUnit studyUnit);

    public void RemoveClass(Schedule schedule, Discipline discipline);
    public void RemoveClassByTime(Schedule schedule, Discipline discipline, string weekFlag, string dayOfWeek, string time);

    public void DismissAllClassesByStudyUnit(Schedule schedule, StudyUnit studyUnit);
    public void DismissClassByStudyUnit(Schedule schedule, Discipline discipline, StudyUnit studyUnit);

    public void DismissClassByStudyUnitAndTime(
        Schedule schedule,
        Discipline discipline,
        StudyUnit studyUnit,
        string weekFlag,
        string dayOfWeek,
        string time);

    public void ChangeClassSchedule(
        Schedule schedule,
        AcademicClass academicClass,
        string newWeekFlag,
        string newDayOfWeek,
        string newTime,
        Tutor newTutor,
        int newClassroom);

    public void ChangeClassTime(
        Schedule schedule,
        AcademicClass academicClass,
        string newWeekFlag,
        string newDayOfWeek,
        string newTime);

    public void ChangeClassTutor(
        Schedule schedule,
        AcademicClass academicClass,
        Tutor newTutor);

    public void ChangeClassRoom(
        Schedule schedule,
        AcademicClass academicClass,
        int newClassroom);

    public AcademicClass GetClassByFullInfo(
        Schedule schedule,
        Discipline discipline,
        string weekFlag,
        string dayOfWeek,
        string time,
        Tutor tutor,
        int classroom);

    public ReadOnlyCollection<AcademicClass> GetClassesListByStudyUnit(Schedule schedule, StudyUnit studyUnit);

    public ReadOnlyCollection<AcademicClass> GetClassesListByDisciplineAndFullTime(
        Schedule schedule,
        Discipline discipline,
        string weekFlag,
        string dayOfWeek,
        string time);

    public ReadOnlyCollection<AcademicClass> GetClassesListByFullTime(
        Schedule schedule,
        string weekFlag,
        string dayOfWeek,
        string time);

    public ReadOnlyCollection<AcademicClass> GetClassesListByDiscipline(Schedule schedule, Discipline discipline);

    public ReadOnlyCollection<AcademicClass> GetClassesListByDisciplineAndStudyUnit(Schedule schedule, Discipline discipline, StudyUnit studyUnit);

    public ReadOnlyCollection<AcademicClass> GetClassesListByStudyUnitAndTime(
        Schedule schedule,
        Discipline discipline,
        StudyUnit studyUnit,
        string weekFlag,
        string dayOfWeek,
        string time);
}
