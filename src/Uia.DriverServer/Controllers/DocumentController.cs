﻿/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 * https://www.w3.org/TR/webdriver/#document
 */
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

using System;
using System.Net.Mime;

using Uia.DriverServer.Domain;
using Uia.DriverServer.Models;

namespace Uia.DriverServer.Controllers
{
    /// <summary>
    /// Controller for handling document-related actions in UI Automation.
    /// </summary>
    /// <param name="domain">The UIA domain interface.</param>
    [Produces(MediaTypeNames.Application.Json)]
    [Route("wd/hub"), Route("/")]
    [ApiController]
    [SwaggerTag(
        description: "Controller for UI Automation document-related actions",
        externalDocsUrl: "https://www.w3.org/TR/webdriver/#document")]
    public class DocumentController(IUiaDomain domain) : ControllerBase
    {
        // Initialize the UIA domain interface
        private readonly IUiaDomain _domain = domain;

        // POST wd/hub/session/{id}/execute/sync
        // POST /session/{id}/execute/sync
        [HttpPost]
        [Route("session/{id}/execute/sync")]
        [SwaggerOperation(
            Summary = "Executes a script in the context of the specified session.",
            Description = "Invokes a script in the session identified by the given session ID.",
            Tags = ["Document"])]
        [SwaggerResponse(200, "Script executed successfully.", typeof(WebDriverResponseModel))]
        [SwaggerResponse(400, "Invalid request. The script data provided is not valid.")]
        [SwaggerResponse(404, "Session not found. The session ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while executing the script.")]
        public IActionResult InvokeScript(
            [SwaggerParameter(Description = "The unique identifier for the session in which the script will be executed.")] string id,
            [SwaggerRequestBody(Description = "The script data to execute, containing the script code and any necessary parameters.")] ScriptInputModel scriptData)
        {
            try
            {
                // Get the session by ID from the domain layer
                var session = _domain.GetSession(id);

                // Invoke the script
                var (statusCode, result) = _domain
                    .DocumentRepository
                    .InvokeScript(session.SessionId, scriptData.Script);

                // Prepare the response model with the result of the script execution
                var value = new WebDriverResponseModel
                {
                    Value = result
                };

                // Return the result as JSON with the appropriate status code
                return new JsonResult(value)
                {
                    StatusCode = statusCode
                };
            }
            catch (Exception e)
            {
                // Log the exception (not shown here) and return a 500 error response
                return StatusCode(
                    statusCode: StatusCodes.Status500InternalServerError,
                    value: $"An error occurred while executing the script: {e}");
            }
        }
    }
}
