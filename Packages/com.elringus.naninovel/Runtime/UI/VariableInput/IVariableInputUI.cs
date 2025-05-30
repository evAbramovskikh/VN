namespace Naninovel.UI
{
    /// <summary>
    /// Implementation is able to present an input UI to set a custom state variable
    /// when requested by <see cref="Commands.InputCustomVariable"/> command.
    /// </summary>
    public interface IVariableInputUI : IManagedUI
    {
        /// <summary>
        /// Shows the UI to input a custom variable.
        /// </summary>
        /// <param name="variableName">Name of custom variable to assign the input value to.</param>
        /// <param name="valueType">Type of the variable value.</param>
        /// <param name="summary">Summary text to show in the UI.</param>
        /// <param name="predefinedValue">A predefined value to set for the input field.</param>
        /// <param name="playOnSubmit">Whether to invoke <see cref="IScriptPlayer.Resume"/> on submit.</param>
        void Show (string variableName, CustomVariableValueType valueType, LocalizableText summary, LocalizableText predefinedValue, bool playOnSubmit);
    }
}
