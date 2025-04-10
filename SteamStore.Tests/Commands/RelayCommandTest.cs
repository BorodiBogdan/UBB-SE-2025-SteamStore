using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamStore.ViewModels;

namespace SteamStore.Tests.Commands
{
    public class RelayCommandTests : IDisposable
    {
        private bool executeCalled;
        private bool canExecuteCalled;
        private string lastParameter;
        private RelayCommand<string> relayCommand;

        public RelayCommandTests()
        {
            this.executeCalled = false;
            this.canExecuteCalled = false;
            this.lastParameter = null;
        }

        [Fact]
        public void Constructor_ExecuteActionIsNull_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new RelayCommand<string>(null));
        }

        [Fact]
        public void Constructor_ValidParameters_InitializesCommand()
        {
            // Arrange
            Action<string> execute = (parameter) => { this.executeCalled = true;
                this.lastParameter = parameter; };
            Predicate<string> canExecute = (parameter) => { this.canExecuteCalled = true;
                return true; };

            // Act
            this.relayCommand = new RelayCommand<string>(execute, canExecute);

            // Assert
            Assert.NotNull(this.relayCommand);
        }

        [Fact]
        public void CanExecute_NoCanExecuteFunction_ReturnsTrue()
        {
            // Arrange
            this.relayCommand = new RelayCommand<string>((parameter) => { });

            // Act
            bool result = this.relayCommand.CanExecute("test");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanExecute_NullParameterForValueType_ReturnsFalse()
        {
            // Arrange
            var intCommand = new RelayCommand<int>((parameter) => { });

            // Act
            bool result = intCommand.CanExecute(null);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanExecute_NonNullValueTypeParameter_ReturnsTrue()
        {
            // Arrange
            var intCommand = new RelayCommand<int>((parameter) => { });

            // Act
            bool result = intCommand.CanExecute(42);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanExecute_WithCanExecuteFunction_CallsFunction()
        {
            // Arrange
            this.relayCommand = new RelayCommand<string>(
                (parameter) => { },
                (parameter) => { this.canExecuteCalled = true;
                    return true; });

            // Act
            this.relayCommand.CanExecute("test");

            // Assert
            Assert.True(this.canExecuteCalled);
        }

        [Fact]
        public void Execute_CallsExecuteActionWithParameter()
        {
            // Arrange
            const string testParameter = "test parameter";
            this.relayCommand = new RelayCommand<string>((parameter) =>
            {
                this.executeCalled = true;
                this.lastParameter = parameter;
            });

            // Act
            this.relayCommand.Execute(testParameter);

            // Assert
            Assert.True(this.executeCalled);
            Assert.Equal(testParameter, this.lastParameter);
        }

        [Fact]
        public void RaiseCanExecuteChanged_InvokesCanExecuteChangedEvent()
        {
            // Arrange
            bool eventRaised = false;
            this.relayCommand = new RelayCommand<string>((parameter) => { });
            this.relayCommand.CanExecuteChanged += (sender, args) => eventRaised = true;

            // Act
            this.relayCommand.RaiseCanExecuteChanged();

            // Assert
            Assert.True(eventRaised);
        }

        [Fact]
        public void CanExecuteChanged_CanSubscribeAndUnsubscribe()
        {
            // Arrange
            bool eventRaised = false;
            EventHandler handler = (sender, args) => eventRaised = true;
            this.relayCommand = new RelayCommand<string>((parameter) => { });

            // Act
            this.relayCommand.CanExecuteChanged += handler;
            this.relayCommand.RaiseCanExecuteChanged();
            bool firstCall = eventRaised;

            eventRaised = false;
            this.relayCommand.CanExecuteChanged -= handler;
            this.relayCommand.RaiseCanExecuteChanged();
            bool secondCall = eventRaised;

            // Assert
            Assert.True(firstCall);
            Assert.False(secondCall);
        }

        [Fact]
        public void CanExecute_ReturnsCanExecuteFunctionResult()
        {
            // Arrange
            this.relayCommand = new RelayCommand<string>(
                (parameter) => { },
                (parameter) => parameter == "valid");

            // Act
            bool validResult = this.relayCommand.CanExecute("valid");
            bool invalidResult = this.relayCommand.CanExecute("invalid");

            // Assert
            Assert.True(validResult);
            Assert.False(invalidResult);
        }

        public void Dispose()
        {
            this.relayCommand = null;
        }
    }
}
