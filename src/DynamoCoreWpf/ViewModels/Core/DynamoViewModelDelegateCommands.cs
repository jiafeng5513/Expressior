using System;
using DelegateCommand = Dynamo.UI.Commands.DelegateCommand;

namespace Dynamo.ViewModels
{
    partial class DynamoViewModel
    {
        private void InitializeDelegateCommands()
        {
            OpenCommand = new DelegateCommand(Open, CanOpen);
            OpenIfSavedCommand = new DelegateCommand(OpenIfSaved, CanOpen);
            OpenRecentCommand = new DelegateCommand(OpenRecent, CanOpenRecent);
            SaveCommand = new DelegateCommand(Save, CanSave);
            SaveAsCommand = new DelegateCommand(SaveAs, CanSaveAs);
            ShowOpenDialogAndOpenResultCommand = new DelegateCommand(ShowOpenDialogAndOpenResult, CanShowOpenDialogAndOpenResultCommand);
            ShowSaveDialogAndSaveResultCommand = new DelegateCommand(ShowSaveDialogAndSaveResult, CanShowSaveDialogAndSaveResult);
            ShowSaveDialogIfNeededAndSaveResultCommand = new DelegateCommand(ShowSaveDialogIfNeededAndSaveResult, CanShowSaveDialogIfNeededAndSaveResultCommand);
            SaveImageCommand = new DelegateCommand(SaveImage, CanSaveImage);
            ShowSaveImageDialogAndSaveResultCommand = new DelegateCommand(ShowSaveImageDialogAndSaveResult, CanShowSaveImageDialogAndSaveResult);
            WriteToLogCmd = new DelegateCommand(o => model.Logger.Log(o.ToString()), CanWriteToLog);
            PostUiActivationCommand = new DelegateCommand(model.PostUIActivation);
            AddNoteCommand = new DelegateCommand(AddNote, CanAddNote);
            AddAnnotationCommand = new DelegateCommand(AddAnnotation,CanAddAnnotation);
            UngroupAnnotationCommand = new DelegateCommand(UngroupAnnotation,CanUngroupAnnotation);
            UngroupModelCommand = new DelegateCommand(UngroupModel,CanUngroupModel);
            AddModelsToGroupModelCommand = new DelegateCommand(AddModelsToGroup, CanAddModelsToGroup);
            AddToSelectionCommand = new DelegateCommand(model.AddToSelection, CanAddToSelection);
            ShowNewFunctionDialogCommand = new DelegateCommand(ShowNewFunctionDialogAndMakeFunction, CanShowNewFunctionDialogCommand);
            SaveRecordedCommand = new DelegateCommand(SaveRecordedCommands, CanSaveRecordedCommands);
            InsertPausePlaybackCommand = new DelegateCommand(ExecInsertPausePlaybackCommand, CanInsertPausePlaybackCommand);
            GraphAutoLayoutCommand = new DelegateCommand(DoGraphAutoLayout, CanDoGraphAutoLayout);
            GoHomeCommand = new DelegateCommand(GoHomeView, CanGoHomeView);
            SelectAllCommand = new DelegateCommand(SelectAll, CanSelectAll);
            HomeCommand = new DelegateCommand(GoHome, CanGoHome);
            NewHomeWorkspaceCommand = new DelegateCommand(MakeNewHomeWorkspace, CanMakeNewHomeWorkspace);
            CloseHomeWorkspaceCommand = new DelegateCommand(CloseHomeWorkspace, CanCloseHomeWorkspace);
            GoToWorkspaceCommand = new DelegateCommand(GoToWorkspace, CanGoToWorkspace);
            DeleteCommand = new DelegateCommand(Delete, CanDelete);
            ExitCommand = new DelegateCommand(Exit, CanExit);
            AlignSelectedCommand = new DelegateCommand(AlignSelected, CanAlignSelected); ;
            UndoCommand = new DelegateCommand(Undo, CanUndo);
            RedoCommand = new DelegateCommand(Redo, CanRedo);
            CopyCommand = new DelegateCommand(_ => model.Copy(), CanCopy);
            PasteCommand = new DelegateCommand(Paste, CanPaste);
            ToggleConsoleShowingCommand = new DelegateCommand(ToggleConsoleShowing, CanToggleConsoleShowing);
            DisplayFunctionCommand = new DelegateCommand(DisplayFunction, CanDisplayFunction);
            SetConnectorTypeCommand = new DelegateCommand(SetConnectorType, CanSetConnectorType);
            DisplayStartPageCommand = new DelegateCommand(DisplayStartPage, CanDisplayStartPage);
            ChangeScaleFactorCommand = new DelegateCommand(p => OnRequestScaleFactorDialog(this, EventArgs.Empty));
            ManagePackagePathsCommand = new DelegateCommand(ManagePackagePaths, o => true);
            OpenBackupPathsCommand = new DelegateCommand(OpnBackupPaths, o => true);
            ShowHideConnectorsCommand = new DelegateCommand(ShowConnectors, CanShowConnectors);
            SelectNeighborsCommand = new DelegateCommand(SelectNeighbors, CanSelectNeighbors);
            ClearLogCommand = new DelegateCommand(ClearLog, CanClearLog);
            PanCommand = new DelegateCommand(Pan, CanPan);
            ZoomInCommand = new DelegateCommand(ZoomIn, CanZoomIn);
            ZoomOutCommand = new DelegateCommand(ZoomOut, CanZoomOut);
            FitViewCommand = new DelegateCommand(FitView, CanFitView);
            EscapeCommand = new DelegateCommand(Escape, CanEscape);
            ImportLibraryCommand = new DelegateCommand(ImportLibrary, CanImportLibrary);
            ShowAboutWindowCommand = new DelegateCommand(ShowAboutWindow, CanShowAboutWindow);
            SetNumberFormatCommand = new DelegateCommand(SetNumberFormat, CanSetNumberFormat);
            CloseGalleryCommand = new DelegateCommand(p => OnRequestShowHideGallery(false), o => true);
            ShowNewPresetsDialogCommand = new DelegateCommand(ShowNewPresetStateDialogAndMakePreset, CanShowNewPresetStateDialog);
            NodeFromSelectionCommand = new DelegateCommand(CreateNodeFromSelection, CanCreateNodeFromSelection);
            CreateANodeByCode = new DelegateCommand(CreateANode, o => true);
        }
        public DelegateCommand OpenIfSavedCommand { get; set; }
        public DelegateCommand OpenCommand { get; set; }
        public DelegateCommand ShowOpenDialogAndOpenResultCommand { get; set; }
        public DelegateCommand WriteToLogCmd { get; set; }
        public DelegateCommand PostUiActivationCommand { get; set; }
        public DelegateCommand AddNoteCommand { get; set; }
        public DelegateCommand AddAnnotationCommand { get; set; }
        public DelegateCommand UngroupAnnotationCommand { get; set; }
        public DelegateCommand UngroupModelCommand { get; set; }
        public DelegateCommand AddModelsToGroupModelCommand { get; set; }
        public DelegateCommand UndoCommand { get; set; }
        public DelegateCommand RedoCommand { get; set; }
        public DelegateCommand CopyCommand { get; set; }
        public DelegateCommand PasteCommand { get; set; }
        public DelegateCommand AddToSelectionCommand { get; set; }
        public DelegateCommand ShowNewFunctionDialogCommand { get; set; }
        public DelegateCommand SaveRecordedCommand { get; set; }
        public DelegateCommand InsertPausePlaybackCommand { get; set; }
        public DelegateCommand GraphAutoLayoutCommand { get; set; }
        public DelegateCommand GoHomeCommand { get; set; }
        public DelegateCommand ManagePackagePathsCommand { get; set; }
        public DelegateCommand OpenBackupPathsCommand { get; set; }
        public DelegateCommand HomeCommand { get; set; }
        public DelegateCommand ExitCommand { get; set; }
        public DelegateCommand ShowSaveDialogIfNeededAndSaveResultCommand { get; set; }
        public DelegateCommand ShowSaveDialogAndSaveResultCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand SaveAsCommand { get; set; }
        public DelegateCommand NewHomeWorkspaceCommand { get; set; }
        public DelegateCommand CloseHomeWorkspaceCommand { get; set; }
        public DelegateCommand GoToWorkspaceCommand { get; set; }
        public DelegateCommand DeleteCommand { get; set; }
        public DelegateCommand AlignSelectedCommand { get; set; }
        public DelegateCommand SelectAllCommand { get; set; }
        public DelegateCommand SaveImageCommand { get; set; }
        public DelegateCommand ShowSaveImageDialogAndSaveResultCommand { get; set; }
        public DelegateCommand ToggleConsoleShowingCommand { get; set; }
        public DelegateCommand DisplayFunctionCommand { get; set; }
        public DelegateCommand SetConnectorTypeCommand { get; set; }
        public DelegateCommand DisplayStartPageCommand { get; set; }
        public DelegateCommand ChangeScaleFactorCommand { get; set; }
        public DelegateCommand ShowHideConnectorsCommand { get; set; }
        public DelegateCommand SelectNeighborsCommand { get; set; }
        public DelegateCommand ClearLogCommand { get; set; }

        public DelegateCommand PanCommand { get; set; }
        public DelegateCommand ZoomInCommand { get; set; }
        public DelegateCommand ZoomOutCommand { get; set; }
        public DelegateCommand FitViewCommand { get; set; }
        public DelegateCommand EscapeCommand { get; set; }
        public DelegateCommand ImportLibraryCommand { get; set; }
        public DelegateCommand ShowAboutWindowCommand { get; set; }
        public DelegateCommand SetNumberFormatCommand { get; set; }
        public DelegateCommand OpenRecentCommand { get; set; }
        public DelegateCommand CreateANodeByCode { get; set; }
        public DelegateCommand CloseGalleryCommand { get; set; }
        public DelegateCommand ShowNewPresetsDialogCommand { get; set; }
        public DelegateCommand NodeFromSelectionCommand { get; set; }       
    }
}
