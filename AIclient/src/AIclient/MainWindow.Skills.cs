using System.Windows;
using System.Windows.Controls;
using AIclient.Models;

namespace AIclient;

public partial class MainWindow
{
    private void RefreshSkills()
    {
        try { SkillBaseBox.Text = File.ReadAllText(skills.SkillPath); SkillManualBox.Text = skills.ReadManual(); RefreshLessons(); SkillsMessage.Text = "技能目录：" + skills.Root; }
        catch (Exception ex) { SkillsMessage.Text = "技能读取失败：" + ex.Message; }
    }
    private void RefreshLessons()
    {
        try { LessonsList.ItemsSource = skills.ReadLessons().OrderByDescending(l => l.CreatedAt).ToList(); }
        catch (Exception ex) { SkillsMessage.Text = "经验读取失败：" + ex.Message; }
    }
    private void SaveSkillsClick(object sender, RoutedEventArgs e)
    {
        try { skills.SaveManual(SkillManualBox.Text); SkillsMessage.Text = "已保存；下一次规划或判断将使用最新补充。"; }
        catch (Exception ex) { SkillsMessage.Text = ex.Message; }
    }
    private void AutoLearnChanged(object sender, RoutedEventArgs e)
    {
        if (skills == null || settings == null || AutoLearnToggle.IsChecked == settings.AutoLearnSkills) return;
        try { settings.AutoLearnSkills = AutoLearnToggle.IsChecked == true; storage.SaveSettings(settings); AdvancedBox.Text = Json.Serialize(settings); SkillsMessage.Text = settings.AutoLearnSkills ? "已开启自动积累，下次任务生效。" : "已关闭自动积累，下次任务生效。已有经验仍可使用。"; }
        catch (Exception ex) { SkillsMessage.Text = ex.Message; }
    }
    private void RefreshSkillsClick(object sender, RoutedEventArgs e) => RefreshLessons();
    private void LessonSelected(object sender, SelectionChangedEventArgs e)
    {
        if (LessonsList.SelectedItem is not TestLesson item) { LessonSource.Text = ""; LessonConditionBox.Clear(); LessonGuidanceBox.Clear(); return; }
        LessonConditionBox.Text = item.Condition; LessonGuidanceBox.Text = item.Guidance;
        LessonSource.Text = $"历史观察建议，需当前证据复核。任务：{item.RunId} / {item.StepId}；证据：{string.Join(", ", item.EvidenceIds)}；台架：{item.Scope[..Math.Min(12, item.Scope.Length)]}";
    }
    private void EditLessonClick(object sender, RoutedEventArgs e) => ChangeLesson(false);
    private void DeleteLessonClick(object sender, RoutedEventArgs e) => ChangeLesson(true);
    private void ChangeLesson(bool delete)
    {
        if (LessonsList.SelectedItem is not TestLesson item) { SkillsMessage.Text = "请先选择经验。"; return; }
        try { skills.UpdateLesson(item.Id, LessonConditionBox.Text, LessonGuidanceBox.Text, delete); RefreshLessons(); SkillsMessage.Text = delete ? "经验已删除。" : "经验已更新，下次分析生效。"; }
        catch (Exception ex) { SkillsMessage.Text = ex.Message; }
    }
}
