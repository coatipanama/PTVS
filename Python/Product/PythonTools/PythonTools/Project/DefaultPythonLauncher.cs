// Python Tools for Visual Studio
// Copyright(c) Microsoft Corporation
// All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the License); you may not use
// this file except in compliance with the License. You may obtain a copy of the
// License at http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED ON AN  *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS
// OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY
// IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
// MERCHANTABLITY OR NON-INFRINGEMENT.
//
// See the Apache Version 2.0 License for specific language governing
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.PythonTools.Debugger;
using Microsoft.PythonTools.Debugger.DebugEngine;
using Microsoft.PythonTools.Infrastructure;
using Microsoft.PythonTools.Interpreter;
using Microsoft.PythonTools.Parsing;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.PythonTools.Project {
    /// <summary>
    /// Implements functionality of starting a project or a file with or without debugging.
    /// </summary>
    sealed class DefaultPythonLauncher : IProjectLauncher {
        private readonly PythonToolsService _pyService;
        private readonly IServiceProvider _serviceProvider;
        private readonly LaunchConfiguration _config;

        public DefaultPythonLauncher(IServiceProvider serviceProvider, LaunchConfiguration config) {
            _serviceProvider = serviceProvider;
            _pyService = _serviceProvider.GetPythonToolsService();
            _config = config;
        }

        public int LaunchProject(bool debug) {
            return Launch(_config, debug);
        }

        public int LaunchFile(string/*!*/ file, bool debug) {
            var config = _config.Clone();
            config.ScriptName = file;
            return Launch(config, debug);
        }

        private int Launch(LaunchConfiguration config, bool debug) {
            if (debug) {
                StartWithDebugger(config);
            } else {
                StartWithoutDebugger(config).Dispose();
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Default implementation of the "Start without Debugging" command.
        /// </summary>
        private Process StartWithoutDebugger(LaunchConfiguration config) {
            _pyService.Logger.LogEvent(Logging.PythonLogEvent.Launch, 0);
            return Process.Start(DebugLaunchHelper.CreateProcessStartInfo(_serviceProvider, config));
        }

        /// <summary>
        /// Default implementation of the "Start Debugging" command.
        /// </summary>
        private void StartWithDebugger(LaunchConfiguration config) {
            _pyService.Logger.LogEvent(Logging.PythonLogEvent.Launch, 1);

            // Historically, we would clear out config.InterpreterArguments at
            // this stage if doing mixed-mode debugging. However, there doesn't
            // seem to be any need to do this, so we now leave them alone.

            using (var dbgInfo = DebugLaunchHelper.CreateDebugTargetInfo(_serviceProvider, config)) {
                dbgInfo.Launch();
            }
        }
    }
}
