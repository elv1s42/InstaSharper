﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.Classes;
using InstaSharper.Examples.Samples;
using InstaSharper.Logger;

namespace InstaSharper.Examples
{
    public class Program
    {
        /// <summary>
        ///     Api instance (one instance per Instagram user)
        /// </summary>
        private static IInstaApi _instaApi;

        private static void Main(string[] args)
        {
            var result = Task.Run(MainAsync).GetAwaiter().GetResult();
            if (result)
                return;
            Console.ReadKey();
        }

        public static async Task<bool> MainAsync()
        {
            try
            {
                Console.WriteLine("Starting demo of InstaSharper project");
                // create user session data and provide login details
                var userSession = new UserSessionData
                {
                    UserName = "user",
                    Password =  "password"
                };

                // create new InstaApi instance using Builder
                _instaApi = InstaApiBuilder.CreateBuilder()
                    .SetUser(userSession)
                    .UseLogger(new DebugLogger(LogLevel.Exceptions)) // use logger for requests and debug messages
                    .SetRequestDelay(TimeSpan.FromSeconds(2))
                    .Build();

                const string stateFile = "state.bin";
                try
                {
                    if (File.Exists(stateFile))
                    {
                        Console.WriteLine("Loading state from file");
                        Stream fs = File.OpenRead(stateFile);
                        fs.Seek(0, SeekOrigin.Begin);
                        _instaApi.LoadStateDataFromStream(fs);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                if (!_instaApi.IsUserAuthenticated)
                {
                    // login
                    Console.WriteLine($"Logging in as {userSession.UserName}");
                    var logInResult = await _instaApi.LoginAsync();
                    if (!logInResult.Succeeded)
                    {
                        Console.WriteLine($"Unable to login: {logInResult.Info.Message}");
                        return false;
                    }
                }
                var state = _instaApi.GetStateDataAsStream();
                using (var fileStream = File.Create(stateFile))
                {
                    state.Seek(0, SeekOrigin.Begin);
                    state.CopyTo(fileStream);
                }

                Console.WriteLine("Press 1 to start basic demo samples");
                Console.WriteLine("Press 2 to start upload photo demo sample");
                Console.WriteLine("Press 3 to start comment media demo sample");
                Console.WriteLine("Press 4 to start stories demo sample");
                Console.WriteLine("Press 5 to start demo with saving state of API instance");
                Console.WriteLine("Press 6 to start messaging demo sample");
                Console.WriteLine("Press 7 to start location demo sample");
                Console.WriteLine("Press 8 to start collections demo sample");

                var samplesMap = new Dictionary<ConsoleKey, IDemoSample>
                {
                    [ConsoleKey.D1] = new Basics(_instaApi),
                    [ConsoleKey.D2] = new UploadPhoto(_instaApi),
                    [ConsoleKey.D3] = new CommentMedia(_instaApi),
                    [ConsoleKey.D4] = new Stories(_instaApi),
                    [ConsoleKey.D5] = new SaveLoadState(_instaApi),
                    [ConsoleKey.D6] = new Messaging(_instaApi),
                    [ConsoleKey.D7] = new LocationSample(_instaApi),
                    [ConsoleKey.D8] = new CollectionSample(_instaApi)

                };
                var key = Console.ReadKey();
                Console.WriteLine(Environment.NewLine);
                if (samplesMap.ContainsKey(key.Key))
                    await samplesMap[key.Key].DoShow();
                Console.WriteLine("Done. Press esc key to exit...");

                key = Console.ReadKey();
                return key.Key == ConsoleKey.Escape;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                // perform that if user needs to logged out
                // var logoutResult = Task.Run(() => _instaApi.LogoutAsync()).GetAwaiter().GetResult();
                // if (logoutResult.Succeeded) Console.WriteLine("Logout succeed");
            }
            return false;
        }
    }
}