﻿// ------------------------------------------------------
// DVTk - The Healthcare Validation Toolkit (www.dvtk.org)
// Copyright © 2009 DVTk
// ------------------------------------------------------
// This file is part of DVTk.
//
// DVTk is free software; you can redistribute it and/or modify it under the terms of the GNU
// Lesser General Public License as published by the Free Software Foundation; either version 3.0
// of the License, or (at your option) any later version. 
// 
// DVTk is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even
// the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser
// General Public License for more details. 
// 
// You should have received a copy of the GNU Lesser General Public License along with this
// library; if not, see <http://www.gnu.org/licenses/>

using System;
using System.Collections;
using System.CodeDom.Compiler;
using System.Reflection;

namespace DvtkScriptSupport
{
    /// <summary>
    /// Interface provided by the script host.
    /// </summary>
    public interface INewScriptHost
    {
        /// <summary>
        /// Source code of the executed script.
        /// </summary>
        string SourceCode { get; set; }
        /// <summary>
        /// Language of the script source and the engine used to execute this code
        /// </summary>
        string Language { get; }

        /// <summary>
        /// Occurs when a compile error report is generated by the CodeDomProvider engine.
        /// </summary>
        event CompilerErrorEventHandler CompilerErrorEvent;

        /// <summary>
        /// Add an event source to the script context.
        /// </summary>
        /// <param name="Name">The name of the event source within the script context.</param>
        /// <param name="Instance">The instance of the event source handed to the script context.</param>
        /// <returns>Success</returns>
        bool AddEventSource(
            string eventSourceName,
            object Instance);

        /// <summary>
        /// Add a reference to the script context.
        /// This allows the use of assemblies from within the script context.
        /// </summary>
        /// <param name="Name">The name of the reference within the script engine.</param>
        /// <param name="AssemblyName">The fullname of the referenced assembly.</param>
        /// <returns>Success</returns>
        bool AddReference(
            string Name,
            string AssemblyName);

        /// <summary>
        /// Add an <see cref="object"/> <c>instance</c> to the script environment.
        /// The <c>instance</c> may be addressed from within a script by means
        /// of the <c>name</c>.
        /// </summary>
        /// <param name="name">global variable name to be used from within a script.</param>
        /// <param name="instance">instance that may be addressed from within a script.</param>
        /// <returns>Success</returns>
        bool AddGlobalInstance(
            string name,
            object instance);

        /// <summary>
        /// Remove a global instance within the <c>name</c> from the script environment.
        /// </summary>
        /// <param name="name">global variable name to be used from within a script.</param>
        /// <returns>Success</returns>
        bool RemoveGlobalInstance(
            string name);

        /// <summary>
        /// Compile the source code within the script context that is set up.
        /// </summary>
        /// <returns>Success code</returns>
        bool Compile();

        /// <summary>
        /// Execute the method which is defined in the script by the moduleName and the methodName.
        /// </summary>
        /// <remarks>
        /// The method needs to be a static method!
        /// </remarks>
        /// <param name="ModuleName">for instance <c>module DvScript ... end module</c></param>
        /// <param name="MethodName">for instance <c>sub Main() .... end sub</c></param>
        /// <param name="Arguments"></param>
        /// <returns></returns>
        object Invoke(
            string ModuleName,
            string MethodName,
            object[] Arguments);
    }

    public abstract class ScriptHost
        : INewScriptHost
    {
        /// <summary>
        /// Provider to be used in the class
        /// </summary>
        private CodeDomProvider provider;

        /// <summary>
        /// Holds compiler parameters
        /// </summary>
        private CompilerParameters parameters;

        /// <summary>
        /// Holds the compiled assembly
        /// </summary>
        private Assembly assembly;

        /// <summary>
        /// Language of the script source and the engine used to execute this code
        /// </summary>
        public string Language
        {
            get { return this._Language; }
        }
        private string _Language;

        /// <summary>
        /// Source code of the executed script.
        /// </summary>
        public string SourceCode { get; set; }

        public string NameSpace { get; set; }

        internal ScriptHost(
            string language,
            string moniker,
            string nameSpace)
        {
            if (language == null) throw new System.ArgumentNullException();
            if (moniker == null) throw new System.ArgumentNullException();
            if (nameSpace == null) throw new System.ArgumentNullException();
            
            try
            {
                // Store the language
                this._Language = language;
                // Create the Script Engine based on the language
                _CreateEngine();
                parameters = new CompilerParameters();
                parameters.GenerateExecutable = false;
                parameters.GenerateInMemory = true;
                parameters.IncludeDebugInformation = false;
                parameters.TreatWarningsAsErrors = false;
                // Set the rootnamespace
                NameSpace = nameSpace;
                // The moniker, or root moniker, 
                // is the unique name by which a script engine is identified
                // Set the moniker to be something relatively unique
                //this._VsaEngine.RootMoniker = moniker;
                // Create a new instance of the VSA Site
                //this._VsaSite = new VsaSite();
                //this._VsaSite.CompilerErrorEvent += new CompilerErrorEventHandler(_VsaSite_CompilerErrorEvent);
                // Get the instance of the site 
                //this._VsaEngine.Site = this._VsaSite;
                // Initialize the engine
                //this._VsaEngine.InitNew();
                // Set the rootnamespace
                //this._VsaEngine.RootNamespace = nameSpace;
                // Set the engine (display) name
                //this._VsaEngine.Name = this._VsaEngine.RootNamespace;
            }
            catch (Exception e)
            {
                throw new System.ApplicationException("A CodeDomProviderException occurred\n" +
                    "ErrorMessage " + e.Message + "\n");
            }
            // Create a reference to system
            this.AddReference("System", "System.dll");
            // Create a reference to Windows
            this.AddReference("Forms", "System.Windows.Forms.dll");
            // Create a reference to Drawing
            this.AddReference("Drawing", "System.Drawing.dll");
            // Create a reference to Xml
            this.AddReference("Xml", "System.Xml.dll");
            // Add an import to system
            this._AddImport("System");
            // Add an import to system
            this._AddImport("System.Windows.Forms");
        }

        /// <summary>
        /// Add an <see cref="object"/> <c>instance</c> to the script environment.
        /// The <c>instance</c> may be addressed from within a script by means
        /// of the <c>name</c>.
        /// </summary>
        /// <param name="name">global variable name to be used from within a script.</param>
        /// <param name="instance">instance that may be addressed from within a script.</param>
        /// <returns>Success</returns>
        public bool AddGlobalInstance(string name, object instance)
        {
            if (name == null) throw new System.ArgumentNullException();
            if (instance == null) throw new System.ArgumentNullException();
            
            if (!provider.IsValidIdentifier(name)) return false;
            //try
            //{
            //    //
            //    // Remove old VsaGlobalItem from engine and site.
            //    //
            //    this.RemoveGlobalInstance(name);
            //    //
            //    // Add new VsaGlobalItem to engine and site.
            //    //
                
            //    this._VsaSite.AddGlobalInstance(name, instance);
            //    Microsoft.Vsa.IVsaGlobalItem vsaGlobalItem =
            //        (Microsoft.Vsa.IVsaGlobalItem)
            //        this._VsaEngine.Items.CreateItem(
            //        name,
            //        Microsoft.Vsa.VsaItemType.AppGlobal,
            //        Microsoft.Vsa.VsaItemFlag.None);
            //    vsaGlobalItem.TypeString = instance.GetType().ToString();
            //}
            //catch (Microsoft.Vsa.VsaException e)
            //{
            //    throw new System.ApplicationException(
            //        string.Format(
            //        (
            //        "A VsaException occurred\n" +
            //        "ErrorCode {0}\n"
            //        ),
            //        e.ErrorCode.ToString()
            //        )
            //        );
            //}
            return true;
        }

        /// <summary>
        /// Remove a global instance within the <c>name</c> from the script environment.
        /// </summary>
        /// <param name="name">global variable name to be used from within a script.</param>
        /// <returns>Success</returns>
        public bool RemoveGlobalInstance(string name)
        {
            if (name == null) throw new System.ArgumentNullException();

            bool success = true;
            //bool success = false;

            //try
            //{
            //    //
            //    // remove existing VsaGlobalItem from engine and site
            //    //
            //    if (this._VsaSite.GetGlobalInstance(name) != null)
            //    {
            //        this._VsaEngine.Items.Remove(name);
            //        this._VsaSite.RemoveGlobalInstance(name);
            //        success = true;
            //    }
            //    else
            //    {
            //        success = false;
            //    }
            //}
            //catch (Microsoft.Vsa.VsaException e)
            //{
            //    throw new System.ApplicationException(
            //        string.Format(
            //        (
            //        "A VsaException occurred\n" +
            //        "ErrorCode {0}\n"
            //        ),
            //        e.ErrorCode.ToString()
            //        )
            //        );
            //}
            return success;
        }

        /// <summary>
        /// Add an event source to the script context.
        /// </summary>
        /// <param name="Name">The name of the event source within the script context.</param>
        /// <param name="Instance">The instance of the event source handed to the script context.</param>
        /// <returns>Success</returns>
        public bool AddEventSource(
            string eventSourceName, object instance)
        {
            if (eventSourceName == null) throw new System.ArgumentNullException();
            if (instance == null) throw new System.ArgumentNullException();

            if (!provider.IsValidIdentifier(eventSourceName)) return false;

            string eventSourceType = instance.GetType().ToString();
            string assemblyName = instance.GetType().Assembly.FullName;
            //
            // Add an event source to the engine
            //
            //this._VsaCodeItem.AddEventSource(eventSourceName, eventSourceType);
            ////
            //// Add the instance to the sites hashtable
            ////
            //this._VsaSite.AddEvent(eventSourceName, instance);
            //
            // JScript.NET Beta 2 has a bug whereby adding an event source actually
            // adds an entry to the item list so adding "Ref" to the Name ensure that
            // theres no naming collisions
            //
            this.AddReference(eventSourceName + "Ref", assemblyName);
            //
            // Make sure the reference gets imported
            // Use split to get the type from the beginning of the Type String
            //
            this._AddImport(instance.GetType().Namespace);

            return true;
        }

        /// <summary>
        /// Add a reference to the script context.
        /// This allows the use of assemblies from within the script context.
        /// </summary>
        /// <param name="Name">The name of the reference within the script engine.</param>
        /// <param name="AssemblyName">The fullname of the referenced assembly.</param>
        /// <returns>Success</returns>
        public bool AddReference(string referenceItemName, string assemblyName)
        {
            if (referenceItemName == null) throw new System.ArgumentNullException();
            if (assemblyName == null) throw new System.ArgumentNullException();

            if (!provider.IsValidIdentifier(referenceItemName)) return false;

            try
            {
                parameters.ReferencedAssemblies.Add(assemblyName);
            }
            catch (Exception e)
            {
                throw new System.ApplicationException("A CodeDomProviderException occurred\n" +
                       "ErrorMessage " + e.Message + "\n");
            }
            return true;
        }

        /// <summary>
        /// Compile the source code within the script context that is set up.
        /// </summary>
        /// <returns>Success code</returns>
        public bool Compile()
        {
            bool compileSuccess = true;
            CompilerResults results = null;
            try
            {
                // Add executing assembly as a reference to the dll to expose interfaces. may not be necessary
                // parameters.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);

                results = provider.CompileAssemblyFromSource(parameters, SourceCode);
                // Check to see if the engine is running
                //if (this._VsaEngine.IsRunning)
                //{
                //    // If it is reset the engine
                //    this._VsaEngine.Reset();
                //    this._VsaEngine.RevokeCache();
                //}
                compileSuccess = results.Errors.Count == 0;
                
                if (compileSuccess)
                {
                    assembly = results.CompiledAssembly;
                }
                
                //if (compileSuccess) compileSuccess = this._VsaEngine.Compile();
                //if (compileSuccess) this._VsaEngine.Run();
            }
            catch (Exception e)
            {
                throw new System.ApplicationException("A CodeDomProviderException occurred\n" +
                       "ErrorMessage " + e.Message + "\n");
            }
            finally
            {
                if (!compileSuccess && results != null)
                {
                    CodeComProvider_CompilerErrorEvent(this, new CompilerErrorEventArgs(results.Errors[0]));
                }
            }

            return compileSuccess;
        }

        /// <summary>
        /// Execute the method which is defined in the script by the moduleName and the methodName.
        /// </summary>
        /// <remarks>
        /// The method needs to be a static method!
        /// </remarks>
        /// <param name="ModuleName">for instance <c>module DvScript ... end module</c></param>
        /// <param name="MethodName">for instance <c>sub Main() .... end sub</c></param>
        /// <param name="Arguments"></param>
        /// <returns></returns>
        public object Invoke(string moduleName, string methodName, object[] arguments)
        {
            if (moduleName == null) throw new System.ArgumentNullException();
            if (methodName == null) throw new System.ArgumentNullException();

            //
            // Engine must be running in order to use reflection to determine
            // provided methods by the Vsa script environment at the run-time.
            //
            if (assembly == null)
            {
                throw new Exception("Engine isn't running!");
            }
            //
            // Create the fullname of the namespace that contains the method
            //
            string fullName = NameSpace + "." + moduleName;
            System.Type moduleType = null;
            try
            {
                //
                // Get the module type from the assembly
                //
                moduleType = assembly.GetType(fullName, true, true);
                //moduleType = this._VsaEngine.Assembly.GetType(fullName, true, true);
            }
            catch (Exception e)
            {
                throw new System.ApplicationException("A CodeDomProviderException occurred\n" +
                       "ErrorMessage " + e.Message + "\n");
            }
            //
            // Get the method from the type
            //
            System.Reflection.MethodInfo methodInfo = moduleType.GetMethod(methodName);
            // Check to see if we can find the method
            if (methodInfo == null)
            {
                throw new Exception("method not found");
            }
            //
            // Call the method with the arguments and return any value.
            // Note: 
            // Invoke is called with argument 'object obj' == null;
            // this means a static call is made!
            //
            object returnObject = null;
            try
            {
                returnObject = methodInfo.Invoke(null, arguments);
            }
            catch (System.Exception e)
            {
                System.String message = System.String.Format("VBScript Execution Failure - Exception: \"{0}\" - Inner Exception: \"{1}\"",
                    e.Message, e.InnerException.Message);
                throw new System.Exception(message, e);
            }
            return returnObject;
        }

        public void Invoke(string moduleName, string methodName)
        {
            if (moduleName == null)
            {
                throw new System.ArgumentNullException("moduleName");
            }

            if (methodName == null)
            {
                throw new System.ArgumentNullException("methodName");
            }


            //
            // Engine must be running in order to use reflection to determine
            // provided methods by the Vsa script environment at the run-time.
            //

            if (assembly == null)
            {
                throw new Exception("Engine isn't running!");
            }


            //
            // Create the fullname of the namespace that contains the method
            //

            string fullName = NameSpace + "." + moduleName;
            System.Type moduleType = null;
            try
            {
                //
                // Get the module type from the assembly
                //

                moduleType = assembly.GetType(fullName, true, true);
            }
            catch
            {
                throw new Exception("Entrypoint (method/sub \"" + methodName + "\" in module \"" + moduleName + "\") not found.");
            }


            //
            // Get the method from the type
            //

            System.Reflection.MethodInfo methodInfo = moduleType.GetMethod(methodName);

            // Check to see if we can find the entry point of the script.
            if (methodInfo == null)
            {
                throw new Exception("Entrypoint (method/sub \"" + methodName + "\" in module \"" + moduleName + "\") not found.");
            }


            //
            // Call the method with an empty list of string arguments first.
            // If that does not succeed, call the same method with no argument.
            //

            bool invokeWithNoArguments = false;

            try
            {
                String[] emptyArray = { };
                ArrayList listContainingExmptyArray = new ArrayList();
                listContainingExmptyArray.Add(emptyArray);

                // Invoke is called with the first argument 'object obj' == null;
                // this means a static call is made!
                methodInfo.Invoke(null, listContainingExmptyArray.ToArray());
            }
            catch (System.Exception theException)
            {
                if (theException is System.Reflection.TargetParameterCountException)
                {
                    // This method, having as parameters a list of strings, does not exist.
                    // Try again with no arguments.
                    invokeWithNoArguments = true;
                }
                else if (theException is System.Reflection.TargetInvocationException)
                {
                    throw (CreateExceptionForTargetParameterCountException(theException as System.Reflection.TargetInvocationException));
                }
                else
                {
                    // Something went wrong while invoking the method that is present.
                    // Rethrow the exception to let the caller deal with this.
                    throw (theException);
                }
            }

            if (invokeWithNoArguments)
            {
                try
                {
                    // Invoke is called with the first argument 'object obj' == null;
                    // this means a static call is made!
                    methodInfo.Invoke(null, null);
                }
                catch (System.Exception theException)
                {
                    if (theException is System.Reflection.TargetParameterCountException)
                    {
                        // The method, having no arguments, also does not exist.
                        throw new Exception("Entrypoint (method/sub \"" + methodName + "\" in module \"" + moduleName + "\"), with either no arguments or a list of string arguments, not found.");
                    }
                    else if (theException is System.Reflection.TargetInvocationException)
                    {
                        throw (CreateExceptionForTargetParameterCountException(theException as System.Reflection.TargetInvocationException));
                    }
                    else
                    {
                        // Something went wrong while invoking the method that is present.
                        // Rethrow the exception to let the caller deal with this.
                        throw (theException);
                    }
                }
            }
        }

        private Exception CreateExceptionForTargetParameterCountException(System.Reflection.TargetInvocationException targetInvocationException)
        {
            Exception exception = null;

            if (targetInvocationException.InnerException != null)
            {
                String message = "";

                message = targetInvocationException.InnerException.Message;

                message += "\r\n\r\nStack trace (of expanded script):\r\n" + targetInvocationException.InnerException.StackTrace;

                exception = new Exception(message);
            }
            else
            {
                exception = new Exception(targetInvocationException.Message);
            }

            return (exception);
        }

        private void _CreateEngine()
        {
            switch (this.Language)
            {
                case "VB":
                case "Visual Basic":
                    //this.provider = CodeDomProvider.CreateProvider("VisualBasic");
                    this.provider = new Microsoft.VisualBasic.VBCodeProvider();
                    break;
                case "JScript":
                case "JScript.NET":
                    //this.provider = CodeDomProvider.CreateProvider("JScript");
                    this.provider = new Microsoft.JScript.JScriptCodeProvider();
                    break;
                default:
                    throw new Exception("Unknown Engine");
            }
        }

        internal protected void _AddImport(string nameSpace)
        {
            if (nameSpace == null) throw new System.ArgumentNullException();

            string importStatement;
            // Should use CodeDOM here really
            // That will be in the next installment
            if (this.Language == "VB")
            {
                importStatement = "imports";
            }
            else
            {
                importStatement = "import";
            }
            importStatement += " " + nameSpace + "\n";
            // Add the imports to the beginning of the script
            this.SourceCode = string.Concat(importStatement, this.SourceCode);
        }

        #region Events Sources (Publishers)
        /// <summary>
        /// Occurs when a compile error report is generated by the vsa engine.
        /// </summary>
        public event CompilerErrorEventHandler CompilerErrorEvent;
        #endregion

        /// <summary>
        /// Cascade the event from the vsa site to external listeners.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CodeComProvider_CompilerErrorEvent(object sender, CompilerErrorEventArgs e)
        {
            //
            // Invoke the delegates
            //
            if (CompilerErrorEvent != null) CompilerErrorEvent(this, e);
        }
    }
}
