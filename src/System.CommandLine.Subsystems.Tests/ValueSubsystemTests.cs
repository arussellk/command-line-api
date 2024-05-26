﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.CommandLine.Directives;
using System.CommandLine.Parsing;
using Xunit;

namespace System.CommandLine.Subsystems.Tests;

public class ValueSubsystemTests
{
    [Fact]
    public void ValueSubsystem_is_activated_by_default()
    {
        CliRootCommand rootCommand = [
            new CliCommand("x")
            {
                new CliOption<string>("--opt1")
            }];
        var configuration = new CliConfiguration(rootCommand);
        var subsystem = new ValueSubsystem();
        var input = "x --opt1 Kirk";
        var args = CliParser.SplitCommandLine(input).ToList();

        Subsystem.Initialize(subsystem, configuration, args);
        var parseResult = CliParser.Parse(rootCommand, args[0], configuration);
        var isActive = Subsystem.GetIsActivated(subsystem, parseResult);

        isActive.Should().BeTrue();
    }

    [Fact]
    public void ValueSubsystem_returns_values_that_are_entered()
    {
        var consoleHack = new ConsoleHack().RedirectToBuffer(true);
        CliOption<int> option = new CliOption<int>("--intValue");
        CliRootCommand rootCommand = [
            new CliCommand("x")
            {
                option
            }];
        var configuration = new CliConfiguration(rootCommand);
        var pipeline = Pipeline.CreateEmpty();
        pipeline.Value = new ValueSubsystem();
        const int expected = 42;
        var input = $"x --intValue {expected}";

        var parseResult = pipeline.Parse(configuration, input); // assigned for debugging
        pipeline.Execute(configuration, input, consoleHack);

        pipeline.Value.GetValue<int>(option).Should().Be(expected);
    }

    [Fact]
    public void ValueSubsystem_returns_default_value_when_no_value_is_entered()
    {
        var consoleHack = new ConsoleHack().RedirectToBuffer(true);
        CliOption<int> option = new CliOption<int>("--intValue");
        CliRootCommand rootCommand = [option];
        var configuration = new CliConfiguration(rootCommand);
        var pipeline = Pipeline.CreateEmpty();
        pipeline.Value = new ValueSubsystem();
        option.SetDefaultValue(43);
        const int expected = 43;
        var input = $"";

        pipeline.Execute(configuration, input, consoleHack);

        pipeline.Value.GetValue<int>(option).Should().Be(expected);
    }


    [Fact]
    public void ValueSubsystem_returns_calculated_default_value_when_no_value_is_entered()
    {
        var consoleHack = new ConsoleHack().RedirectToBuffer(true);
        CliOption<int> option = new CliOption<int>("--intValue");
        CliRootCommand rootCommand = [option];
        var configuration = new CliConfiguration(rootCommand);
        var pipeline = Pipeline.CreateEmpty();
        pipeline.Value = new ValueSubsystem();
        var x = 42;
        option.SetDefaultValueCalculation(() => x + 2);
        const int expected = 44;
        var input = "";

        var parseResult = pipeline.Parse(configuration, input); // assigned for debugging
        pipeline.Execute(configuration, input, consoleHack);

        pipeline.Value.GetValue<int>(option).Should().Be(expected);
    }
}