using System;
using System.Reactive;
using System.Reactive.Linq;
using Instrumentation.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Instrumentation.Dialogs
{
    public class MessageDialogViewModel : ViewModelBase
    {
        public bool IsOkButtonShown { get; }

        public bool IsCancelButtonShown { get; }

        public bool IsYesButtonShown { get; }

        public bool IsNoButtonShown { get; }

        public string Message { get; }

        public MessageDialogIcon Icon { get; }

        [Reactive] public MessageDialogResult? Result { get; private set; }

        public string OkButtonText { get; }

        public string CancelButtonText { get; }

        public string YesButtonText { get; }

        public string NoButtonText { get; }

        public ReactiveCommand<Unit, MessageDialogResult> Ok { get; }
        public ReactiveCommand<Unit, MessageDialogResult> Cancel { get; private set; }
        public ReactiveCommand<Unit, MessageDialogResult> Yes { get; private set; }
        public ReactiveCommand<Unit, MessageDialogResult> No { get; private set; }

        public ReactiveCommand<Unit, bool> Close { get; }

        public MessageDialogViewModel(
            string message,
            MessageDialogIcon icon,
            string okButtonText = "OK",
            string cancelButtonText = "Отмена",
            string yesButtonText = "Да",
            string noButtonText = "Нет",
            params MessageDialogButton[] buttons)
        {
            Message = message;
            OkButtonText = okButtonText;
            CancelButtonText = cancelButtonText;
            YesButtonText = yesButtonText;
            NoButtonText = noButtonText;
            Icon = icon;


            foreach (var button in buttons)
                switch (button)
                {
                    case MessageDialogButton.Yes:
                        IsYesButtonShown = true;
                        break;
                    case MessageDialogButton.No:
                        IsNoButtonShown = true;
                        break;
                    case MessageDialogButton.Ok:
                        IsOkButtonShown = true;
                        break;
                    case MessageDialogButton.Cancel:
                        IsCancelButtonShown = true;
                        break;
                }

            Ok = ReactiveCommand.Create(() => MessageDialogResult.Ok);

            Cancel = ReactiveCommand.Create(() => MessageDialogResult.Cancel);

            Yes = ReactiveCommand.Create(() => MessageDialogResult.Yes);

            No = ReactiveCommand.Create(() => MessageDialogResult.No);

            Close = ReactiveCommand.Create(() => true);

            var commands = Ok.Concat(Ok)
                .Concat(Cancel)
                .Concat(Yes)
                .Concat(No);

            commands.Subscribe(x => { Result = x; });

            commands
                .SelectMany(_ => Close.Execute())
                .Subscribe();
        }
    }
}
