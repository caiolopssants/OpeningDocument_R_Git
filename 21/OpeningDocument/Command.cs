using System;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
namespace OpeningDocument
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            using (OpenFileDialog oFD = new OpenFileDialog {Multiselect = true, Filter = "Revit Project/ Family|*.rvt;*.rte;*.rfa;*.rft"})
            {   
                if (oFD.ShowDialog() == DialogResult.OK && oFD.FileNames.Length > 0)
                {
                    DialogResult dialogResult = oFD.FileNames.Any(path => Path.GetExtension(path) == ".rvt") ? MessageBox.Show("Do you wanna open like a detached document?", "Opening document mode", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) : DialogResult.None;
                    if (dialogResult == DialogResult.Cancel) { return Result.Cancelled; }
                    DialogResult discartWorksetDialogResult = dialogResult == DialogResult.Yes ? MessageBox.Show("Do you wanna preserve the document worksets", "Workset preservation", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) : DialogResult.None;
                    if (discartWorksetDialogResult == DialogResult.Cancel) { return Result.Cancelled; }

                    EventHandler<DialogBoxShowingEventArgs>  eventHandler = SetPopUpEvent(commandData);
                    foreach (string path in oFD.FileNames)
                    {
                        try
                        {
                            DetachFromCentralOption detachFromCentral = (dialogResult == DialogResult.Yes && Path.GetExtension(path) == ".rvt") ?
                                                                            (discartWorksetDialogResult == DialogResult.Yes) ?
                                                                                DetachFromCentralOption.DetachAndPreserveWorksets :
                                                                                    DetachFromCentralOption.DetachAndDiscardWorksets :
                                                                                        DetachFromCentralOption.DoNotDetach;
                            commandData.Application.OpenAndActivateDocument(ModelPathUtils.ConvertUserVisiblePathToModelPath(path),
                                new OpenOptions
                                {
                                    AllowOpeningLocalByWrongUser = true,
                                    DetachFromCentralOption = detachFromCentral
                                },
                                true);
                        }
                        catch(Exception error)
                        {
                            MessageBox.Show($"Error: {error.Message}\nTime: {DateTime.Now.ToString()}");
                        }
                    }
                    RemovePopUpEvent(commandData, eventHandler);
                }
            }
                return Result.Succeeded;
        }
        private void RemovePopUpEvent(ExternalCommandData commandData, EventHandler<DialogBoxShowingEventArgs> eventHandler) => commandData.Application.DialogBoxShowing -= eventHandler;
        private EventHandler<DialogBoxShowingEventArgs> SetPopUpEvent(ExternalCommandData commandData)
        {
            EventHandler < DialogBoxShowingEventArgs > eventHandler = new EventHandler<DialogBoxShowingEventArgs>(PopUpControlEvent);
            commandData.Application.DialogBoxShowing += eventHandler;
            return eventHandler;
            void PopUpControlEvent(object sender, DialogBoxShowingEventArgs e)
            {
                if (!e.OverrideResult((int)TaskDialogResult.Ok))
                {
                    e.OverrideResult((int)TaskDialogResult.Cancel);
                    MessageBox.Show($"Canceled process because of this message: {((TaskDialogShowingEventArgs)e).Message}", "Process canceled");
                }
                #region Code disabled
                //Autodesk.Revit.UI.Events.TaskDialogShowingEventArgs e1 = e as Autodesk.Revit.UI.Events.TaskDialogShowingEventArgs;
                //File.AppendAllText(@"G:\messages.txt",
                //    $"Message: \n" +
                //    $"Cancellable: {e.Cancellable}\n" +
                //    $"DialogId: {e.DialogId}\n" +
                //    $"IsValidObject: {e.IsValidObject}\n" +                
                //    $"-------------------------------------------\n" +
                //    $"Message: {e1.Message}\n" +
                //    $"Cancellable: {e.Cancellable}\n" +
                //    $"DialogId: {e.DialogId}\n" +
                //    $"IsValidObject: {e.IsValidObject}\n" +
                //    $"\n\n\n\n");
                //MessageBox.Show($"{e.OverrideResult((int)TaskDialogResult.Cancel)} 1");
                //MessageBox.Show($"{e.OverrideResult((int)TaskDialogResult.Close)} 2");
                //MessageBox.Show($"{e.OverrideResult((int)TaskDialogResult.CommandLink1)} 3");
                //MessageBox.Show($"{e.OverrideResult((int)TaskDialogResult.CommandLink2)} 4");
                //MessageBox.Show($"{e.OverrideResult((int)TaskDialogResult.CommandLink3)} 5");
                //MessageBox.Show($"{e.OverrideResult((int)TaskDialogResult.CommandLink4)} 6");
                //MessageBox.Show($"{e.OverrideResult((int)TaskDialogResult.No)} 7");
                //MessageBox.Show($"{e.OverrideResult((int)TaskDialogResult.None)} 8");
                //MessageBox.Show($"{e.OverrideResult((int)TaskDialogResult.Ok)} 9");
                //MessageBox.Show($"{e.OverrideResult((int)TaskDialogResult.Retry)} 10");
                //MessageBox.Show($"{e.OverrideResult((int)TaskDialogResult.Yes)} 11");
                #endregion
            }
        }
       
    }
}
