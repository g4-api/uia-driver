﻿/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

using Uia.DriverServer.Extensions;
using Uia.DriverServer.Marshals;
using Uia.DriverServer.Marshals.Models;
using Uia.DriverServer.Models;

#pragma warning disable S3011 // Suppresses warnings about using reflection to access members, which is necessary for dynamic method invocation in this context.
#pragma warning disable S1144, IDE0051, RCS1213 // Suppresses warnings about unused private methods, which are dynamically invoked using reflection.
namespace Uia.DriverServer.Domain
{
    /// <summary>
    /// Implements the IActionsRepository interface to handle sending actions to the system.
    /// </summary>
    public class ActionsRepository : IActionsRepository
    {
        /// <inheritdoc />
        public void SendActions(UiaSessionResponseModel session, ActionsModel actionsModel)
        {
            foreach (var action in actionsModel.Actions)
            {
                var actions = FilterActions(action);

                // Iterate through each input in the sequence.
                foreach (var input in actions)
                {
                    // Send the input to the system.
                    SendInput(instance: this, session: session, inputData: input);

                    // Sleep for the specified duration.
                    Thread.Sleep(TimeSpan.FromMilliseconds(10));
                }
            }
        }

        // Sends input to the specified UI automation session based on the provided input data.
        private static void SendInput(ActionsRepository instance, UiaSessionResponseModel session, Dictionary<string, object> inputData)
        {
            // Check if the input data contains a "type" key, and retrieve its value
            if (!inputData.TryGetValue("type", out object type))
            {
                // If "type" is not found, return early
                return;
            }

            // Retrieve all non-public, static methods from the ActionsRepository type
            var inputMethods = instance
                .GetType()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static);

            // Find the method that matches the input type name (case-insensitive)
            var inputMethod = Array.Find(inputMethods, m => m.Name.Equals($"{type}", StringComparison.OrdinalIgnoreCase));

            // Create a new InputDataModel with the provided input data and session
            var inputDataModel = new InputDataModel
            {
                Data = inputData,
                Session = session
            };

            // Invoke the input method with the input data model
            inputMethod.Invoke(instance, [inputDataModel]);
        }

        // Handles the key down event by sending the appropriate input to the system.
        private static void KeyDown(InputDataModel inputData)
        {
            // Check if the input data contains a "value" key
            if (!inputData.Data.TryGetValue("value", out object value))
            {
                // If "value" is not found, return early
                return;
            }

            // Convert the value to a sequence of keyboard input events
            var inputs = $"{value}".ConvertToInputs(KeyEvent.KeyDown).ToArray();

            // Send the input events to the system
            User32.SendInput(inputs);
        }

        // Handles the key up event by sending the appropriate input to the system.
        private static void KeyUp(InputDataModel inputData)
        {
            // Check if the input data contains a "value" key
            if (!inputData.Data.TryGetValue("value", out object value))
            {
                // If "value" is not found, return early
                return;
            }

            // Convert the value to a sequence of keyboard input events
            var inputs = $"{value}".ConvertToInputs(KeyEvent.KeyUp).ToArray();

            // Send the input events to the system
            User32.SendInput(inputs);
        }

        // Handles the pause event by pausing the execution for the specified duration.
        private static void Pause(InputDataModel inputData)
        {
            // Check if the input data contains a "duration" key
            if (!inputData.Data.TryGetValue("duration", out object value))
            {
                // If "duration" is not found, return early
                return;
            }

            // Try to parse the duration value from the input data
            var isDuration = int.TryParse($"{value}", out int durationOut);

            // If parsing is successful, use the parsed value; otherwise, default to 0
            var duration = isDuration ? durationOut : 0;

            // Pause the execution for the specified duration
            Thread.Sleep(TimeSpan.FromMilliseconds(duration));
        }

        // Handles the pointer down event by sending the appropriate mouse input to the system.
        private static void PointerDown(InputDataModel inputData)
        {
            // Check if the input data contains a "button" key
            if (!inputData.Data.TryGetValue("button", out object value))
            {
                // If "button" is not found, return early
                return;
            }

            // Try to parse the button value from the input data
            _ = int.TryParse($"{value}", out int buttonOut);

            // Determine the mouse event based on the button value
            var mouseEvent = buttonOut == 2 ? MouseEvent.RightDown : MouseEvent.LeftDown;

            // Create a new mouse input for the session based on the mouse event
            var input = inputData.Session.NewMouseInput(mouseEvent);

            // Send the input event to the system
            User32.SendInput(input);
        }

        private static void PointerMove(InputDataModel inputData)
        {
        }

        // Handles the pointer up event by sending the appropriate mouse input to the system.
        private static void PointerUp(InputDataModel inputData)
        {
            // Check if the input data contains a "button" key
            if (!inputData.Data.TryGetValue("button", out object value))
            {
                // If "button" is not found, return early
                return;
            }

            // Try to parse the button value from the input data
            _ = int.TryParse($"{value}", out int buttonOut);

            // Determine the mouse event based on the button value
            var mouseEvent = buttonOut == 2 ? MouseEvent.RightUp : MouseEvent.LeftUp;

            // Create a new mouse input for the session based on the mouse event
            var input = inputData.Session.NewMouseInput(mouseEvent);

            // Send the input event to the system
            User32.SendInput(input);
        }

        // Filters the actions from the given action model to exclude unnecessary pause actions.
        private static List<Dictionary<string, object>> FilterActions(ActionsModel.ActionModel actionModel)
        {
            // List to hold the filtered actions
            var filteredActions = new List<Dictionary<string, object>>();

            // Iterate over each action in the action model
            foreach (var item in actionModel.Actions)
            {
                // Extract the type of the action as a string
                var type = $"{item["type"]}";

                // Try to get the duration value from the action dictionary
                var durationValue = item.TryGetValue("duration", out object d) ? d : 0;

                // Check if the duration value can be parsed to an integer
                var isDuration = int.TryParse($"{durationValue}", out int durationOut);

                // Add the action to the filtered list if it's not a pause or if it's a pause with a non-zero duration
                if (!type.Equals("pause") || (isDuration && durationOut != 0))
                {
                    filteredActions.Add(item);
                }
            }

            // Return the list of filtered actions
            return filteredActions;
        }

        private sealed class InputDataModel
        {
            public Dictionary<string, object> Data { get; set; }
            public UiaSessionResponseModel Session { get; set; }
        }
    }
}
