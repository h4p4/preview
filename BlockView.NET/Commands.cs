// (C) Copyright 2002-2007 by Autodesk, Inc. 
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted, 
// provided that the above copyright notice appears in all copies and 
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting 
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC. 
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is subject to 
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.
//

using System;
using Autodesk.AutoCAD.Runtime;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.Windows;

[assembly: CommandClass(typeof(BlockView.NET.asdkCommands))]

//////////////////////////////////////////////////////////////////////////
// by Fenton Webb, DevTech, Autodesk. 23/05/2008
namespace BlockView.NET
{
  /// <summary>
  /// Summary description for asdkCommands.
  /// </summary>
  public class asdkCommands
  {
    private static Autodesk.AutoCAD.Windows.PaletteSet _ps;
    private static GSPreviewWPFUC _uc;

    public asdkCommands()
    {
    }

    [CommandMethod("bview")]
    static public void bview()
    {
      // create a new instance of the dialog
      using (BlockViewDialog blockViewDlg = new BlockViewDialog())
      {
        // now show it as a model form in AutoCAD
        AcadApp.ShowModalDialog(blockViewDlg);
        
      }
    }

    [CommandMethod("bviewpal")]
    static public void bviewpal() 
    {
         if (_ps == null)
        {
            _ps = new PaletteSet("WPF Palette");
            _ps.Size = new System.Drawing.Size(400, 600);
            _ps.DockEnabled = (DockSides)((int)DockSides.Left + (int)DockSides.Right);
            _uc = new GSPreviewWPFUC();
            _ps.AddVisual("Preview", _uc);
            _ps.PaletteSetDestroy += _ps_PaletteSetDestroy;
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.BeginDocumentClose += MdiActiveDocument_BeginDocumentClose;
        }
        _ps.Visible = true;
    }

    static void MdiActiveDocument_BeginDocumentClose(object sender, Autodesk.AutoCAD.ApplicationServices.DocumentBeginCloseEventArgs e)
    {
        _ps.Dispose();
        _ps = null;
    }

    static void _ps_PaletteSetDestroy(object sender, EventArgs e)
    {
        if (_uc != null)
        {
            _uc.Cleanup();
        }
    }
  }
}