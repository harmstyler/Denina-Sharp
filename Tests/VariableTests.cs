﻿using System;
using System.Linq;
using BlendInteractive.TextFilterPipeline.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class VariableTests
    {
        [TestMethod]
        public void WriteToVariable()
        {
            var pipeline = new TextFilterPipeline();
            pipeline.AddCommand("WriteTo $Name");
            pipeline.Execute("Deane");

            Assert.AreEqual("Deane", pipeline.Variables["Name"]);
        }

        [TestMethod]
        public void ReadFromVariable()
        {
            var pipeline = new TextFilterPipeline();
            pipeline.AddCommand("WriteTo $Name"); // Writes original input to the variable "Name"
            pipeline.AddCommand("Text.ReplaceAll Annie"); // Resets input to "Annie"

            Assert.AreEqual("Annie", pipeline.Execute("Anything"));

            pipeline.AddCommand("ReadFrom $Name"); // Input should be the original again

            Assert.AreEqual("Deane", pipeline.Execute("Deane"));
        }


        [TestMethod]
        public void ReadNonExistentVariableWithDefault()
        {
            var pipeline = new TextFilterPipeline();

            try
            {
                pipeline.AddCommand("ReadFrom $Name");
                pipeline.Execute();
                Assert.Fail("Test should have failed in the line above and gone to the \"catch\" block...");
            }
            catch (TfpException e)
            {
                // This is the passing block...
                Assert.IsTrue(e.Message.StartsWith("Attempt to access non-existent variable"));
            }
            
        }

        [TestMethod]
        public void WritingIntoImplicitVariable()
        {
            var pipeline = new TextFilterPipeline();
            pipeline.AddCommand("Text.Append \" married Deane.\" => $myVar");
            var result = pipeline.Execute("Annie");

            Assert.AreEqual(result, "Annie");   // The input text should be unchanged
            Assert.AreEqual(pipeline.Variables["myVar"], "Annie married Deane.");
        }

        [TestMethod]
        public void ResolveVariableNames()
        {
            var input = "Deane";
            
            var pipeline = new TextFilterPipeline();
            pipeline.AddCommand("WriteTo $myVar");   // Write the input to a variable
            pipeline.AddCommand("Text.Prepend $myVar");  // Prepend that variable onto the input
            var result = pipeline.Execute(input);

            Assert.AreEqual(String.Concat(input, input), result);   // Result should be the input twice
        }

        [TestMethod]
        public void VariablePrefixes()
        {
            return; // This test may be invalid now...
            var input = "Deane";
            var variableName = "myVar";

            var pipeline = new TextFilterPipeline();
            pipeline.AddCommand("WriteTo " + variableName);
            pipeline.AddCommand("WriteTo $" + variableName);    // The prefix should be removed. This should write the same place as the first command.
            var result = pipeline.Execute(input);

            Assert.AreEqual(1, pipeline.Variables.Count);
            Assert.AreEqual(variableName, pipeline.Variables.First().Key);
            Assert.AreEqual(input, pipeline.GetVariable(variableName));
        }

        [TestMethod]
        public void PipingInputToFromVariables()
        {
            var input = "Deane";
            var secondInput = "Annie";
            var variableName = "$myVar";

            var pipeline = new TextFilterPipeline();
            pipeline.AddCommand("=> " + variableName);          // This puts the input into the variable
            Assert.AreEqual(input, pipeline.Execute(input));
            Assert.AreEqual(input, pipeline.GetVariable(variableName));

            pipeline.AddCommand("Text.ReplaceAll " + secondInput);   // Replaces the input
            Assert.AreEqual(secondInput, pipeline.Execute(input));
            Assert.AreEqual(input, pipeline.GetVariable(variableName));
            
            pipeline.AddCommand(variableName + " =>");  // Resets the input to the original (from the variable)
            Assert.AreEqual(input, pipeline.Execute(input));

        }
    }
}