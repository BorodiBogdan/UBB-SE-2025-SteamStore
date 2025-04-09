using SteamStore.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamStore.Tests.Commands
{
    public class RelayCommandWithoutParameterTest : IDisposable
    {
        private bool executeCalled;
        private bool canExecuteCalled;
        private RelayCommandWithoutParameter relayCommand;
        public RelayCommandWithoutParameterTest()
        {
            this.executeCalled = false;
            this.canExecuteCalled = false;
        }

        [Fact]
        public void Constructor_ExecuteActionIsNull_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new RelayCommandWithoutParameter(null));
            Assert.Equal("execute", exception.ParamName);
        }

        [Fact]
        public void Constructor_ExecuteActionNotNull_AssignsAction()
        {
            // Arrange
            Action execute = () => this.executeCalled = true;

            // Act
            this.relayCommand = new RelayCommandWithoutParameter(execute);
            this.relayCommand.Execute(null);

            // Assert
            Assert.True(this.executeCalled);
        }

        [Fact]
        public void Constructor_ValidParameters_InitializesCommand()
        {
            // Arrange
            Action execute = () => this.executeCalled = true;
            Func<bool> canExecute = () => { this.canExecuteCalled = true; return true; };

            // Act
            this.relayCommand = new RelayCommandWithoutParameter(execute, canExecute);

            // Assert
            Assert.NotNull(this.relayCommand);

            // Verify both delegates are properly assigned
            this.relayCommand.Execute(null);
            Assert.True(this.executeCalled);

            bool canExecuteResult = this.relayCommand.CanExecute(null);
            Assert.True(this.canExecuteCalled);
            Assert.True(canExecuteResult);
        }

        [Fact]
        public void CanExecute_NoCanExecuteFunction_ReturnsTrue()
        {
            // Arrange
            this.relayCommand = new RelayCommandWithoutParameter(() => { });

            // Act
            bool result = this.relayCommand.CanExecute(null);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanExecute_WithCanExecuteFunctionReturnsFalse_ReturnsFalse()
        {
            // Arrange
            this.relayCommand = new RelayCommandWithoutParameter(
                () => { },
                () => false);

            // Act
            bool result = this.relayCommand.CanExecute(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanExecute_WithCanExecuteFunction_CallsFunction()
        {
            // Arrange
            this.relayCommand = new RelayCommandWithoutParameter(
                () => { },
                () => { this.canExecuteCalled = true; return true; });

            // Act
            this.relayCommand.CanExecute(null);

            // Assert
            Assert.True(this.canExecuteCalled);
        }

        [Fact]
        public void Execute_CallsExecuteAction()
        {
            // Arrange
            this.relayCommand = new RelayCommandWithoutParameter(() => this.executeCalled = true);

            // Act
            this.relayCommand.Execute(null);

            // Assert
            Assert.True(this.executeCalled);
        }

        [Fact]
        public void RaiseCanExecuteChanged_InvokesCanExecuteChangedEvent()
        {
            // Arrange
            bool eventRaised = false;
            this.relayCommand = new RelayCommandWithoutParameter(() => { });
            this.relayCommand.CanExecuteChanged += (sender, args) => eventRaised = true;

            // Act
            this.relayCommand.RaiseCanExecuteChanged();

            // Assert
            Assert.True(eventRaised);
        }

        [Fact]
        public void RaiseCanExecuteChanged_NoHandlersAttached_DoesNotThrow()
        {
            // Arrange
            this.relayCommand = new RelayCommandWithoutParameter(() => { });

            // Act & Assert
            var exception = Record.Exception(() => this.relayCommand.RaiseCanExecuteChanged());
            Assert.Null(exception);
        }

        [Fact]
        public void CanExecuteChanged_CanSubscribeAndUnsubscribe()
        {
            // Arrange
            bool eventRaised = false;
            EventHandler handler = (sender, args) => eventRaised = true;
            this.relayCommand = new RelayCommandWithoutParameter(() => { });

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
        public void Execute_WithParameter_CallsExecuteAction()
        {
            // Arrange
            this.relayCommand = new RelayCommandWithoutParameter(() => this.executeCalled = true);

            // Act
            this.relayCommand.Execute("some parameter");

            // Assert
            Assert.True(this.executeCalled);
        }

        [Fact]
        public void CanExecute_WithParameter_CallsCanExecuteFunction()
        {
            // Arrange
            this.relayCommand = new RelayCommandWithoutParameter(
                () => { },
                () => { this.canExecuteCalled = true; return true; });

            // Act
            this.relayCommand.CanExecute("some parameter");

            // Assert
            Assert.True(this.canExecuteCalled);
        }

        public void Dispose()
        {
            this.relayCommand = null;
        }
    }
}