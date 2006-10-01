/*
    Copyright 2005 - 2006 Roman Plasil
	http://foo-title.sourceforge.net
    This file is part of foo_title.

    foo_title is free software; you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation; either version 2.1 of the License, or
    (at your option) any later version.

    foo_title is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with foo_title; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/
#pragma once
#include "Stdafx.h"

using namespace System;

namespace fooManagedWrapper
{
	// a managed wrapper for metadb_handle
	public ref class MetaDBHandle 
	{
	public:
		MetaDBHandle(metadb_handle *src);
		!MetaDBHandle();
		~MetaDBHandle();

		metadb_handle *GetHandle() { return handle; };
		String ^GetPath();
	private:
		metadb_handle *handle;
	};

	// a managed wrapper for foobar's console
	public ref class Console {
	public:
		static void Error(String ^a);
		static void Write(String ^a);
		static void Warning(String ^a);

	};

	public interface class IPlayControl {
		enum class StopReason {
			stop_reason_user = 0,
			stop_reason_eof = 1,
			stop_reason_starting_another = 2,
			stop_reason_shutting_down = 3
		};
		String ^FormatTitle(MetaDBHandle ^dbHandle, String ^spec);
	};

	public ref class CPlayControl : public IPlayControl {
	public:
	
		virtual String ^FormatTitle(MetaDBHandle ^dbHandle, String ^spec);
	};


	// this is the main entry point for each dotnet_ component - one class must implement it
	public interface class IComponentClient {
		
		// the component class must create all services in this method
		void Create();

		// this also gives the component an IPlayControl implementation
		void OnInit(IPlayControl ^a);
		void OnQuit();

		// these are the play callbacks
		void OnPlaybackNewTrack(MetaDBHandle ^h);
		void OnPlaybackTime(double time);
		void OnPlaybackPause(bool state);
		void OnPlaybackStop(IPlayControl::StopReason reason);
	};

}