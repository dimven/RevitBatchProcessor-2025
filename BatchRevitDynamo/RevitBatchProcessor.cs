﻿//
// Revit Batch Processor
//
// Copyright (c) 2017  Daniel Rumery, BVN
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using BatchRvtUtil;

namespace BatchRevitDynamo
{
    public static class RevitBatchProcessor
    {
        /// <summary>
        /// Runs a Revit Batch Processing task.
        /// </summary>
        /// <param name="taskScriptFilePath"></param>
        /// <param name="revitFileListFilePath"></param>
        /// <param name="useRevitVersion"></param>
        /// <param name="revitSessionOption"></param>
        /// <param name="centralFileOpenOption"></param>
        /// <param name="deleteLocalAfter"></param>
        /// <param name="discardWorksetsOnDetach"></param>
        /// <returns>Full path to the generated log file.</returns>
        public static string RunTask(
                bool toggleToExecute, // TODO: reconsider if this is needed here.
                string taskScriptFilePath,
                string revitFileListFilePath,
                UseRevitVersion useRevitVersion,
                CentralFileOpenOption centralFileOpenOption,
                bool discardWorksetsOnDetach,
                bool deleteLocalAfter
            )
        {
            var batchRvtFolderPath = BatchRvt.GetBatchRvtFolderPath();

            var taskRevitVersion = (
                    useRevitVersion == UseRevitVersion.Revit2016 ? 
                    RevitVersion.SupportedRevitVersion.Revit2016 :
                    useRevitVersion == UseRevitVersion.Revit2017 ?
                    RevitVersion.SupportedRevitVersion.Revit2017 :
                    useRevitVersion == UseRevitVersion.Revit2018 ?
                    RevitVersion.SupportedRevitVersion.Revit2018 :
                    RevitVersion.SupportedRevitVersion.Revit2018 // NOTE: can be any version since UseRevitVersion is set to RevitFileVersion.
                );

            var batchRvtRevitFileProcessingOption = (
                    useRevitVersion == UseRevitVersion.RevitFileVersion ?
                    BatchRvt.RevitFileProcessingOption.UseFileRevitVersionIfAvailable :
                    BatchRvt.RevitFileProcessingOption.UseSpecificRevitVersion
                );

            var batchRvtCentralFileOpenOption = (
                    centralFileOpenOption == CentralFileOpenOption.CreateNewLocal ?
                    BatchRvt.CentralFileOpenOption.CreateNewLocal :
                    BatchRvt.CentralFileOpenOption.Detach
                );

            var batchRvtSettings = BatchRvtSettings.Create(
                    taskScriptFilePath,
                    revitFileListFilePath,
                    batchRvtCentralFileOpenOption,
                    deleteLocalAfter,
                    discardWorksetsOnDetach,
                    BatchRvt.RevitSessionOption.UseSeparateSessionPerFile,
                    batchRvtRevitFileProcessingOption,
                    taskRevitVersion
                );

            batchRvtSettings.SaveToAppDomainData();

            BatchRvt.ExecuteMonitorScript(batchRvtFolderPath);

            var logFilePath = BatchRvt.GetAppDomainDataLogFilePath();

            if (!string.IsNullOrWhiteSpace(logFilePath))
            {
                var plainTextLogFilePath = Path.Combine(
                        Path.GetDirectoryName(logFilePath),
                        Path.GetFileNameWithoutExtension(logFilePath) + ".txt"
                    );

                File.WriteAllLines(
                        plainTextLogFilePath,
                        LogFile.ReadLinesAsPlainText(logFilePath)
                    );

                logFilePath = plainTextLogFilePath;
            }
            
            return logFilePath;
        }
    }

    // NOTE: Dynamo does not support Revit versions earlier than 2016.
    public enum UseRevitVersion { RevitFileVersion = 0, Revit2016 = 1, Revit2017 = 2, Revit2018 = 3 }
    public enum RevitSessionOption { UseSeparateSessionPerFile = 0, UseSameSessionForFilesOfSameVersion = 1 }
    public enum CentralFileOpenOption { Detach = 0, CreateNewLocal = 1 }
}
