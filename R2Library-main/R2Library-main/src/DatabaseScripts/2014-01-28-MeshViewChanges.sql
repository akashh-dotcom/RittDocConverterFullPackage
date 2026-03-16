USE [DEV_RIT001]
GO

IF  EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_DiagramPaneCount' , N'SCHEMA',N'dbo', N'VIEW',N'MeshDiseaseTerms', NULL,NULL))
EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPaneCount' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'MeshDiseaseTerms'

GO

IF  EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_DiagramPane2' , N'SCHEMA',N'dbo', N'VIEW',N'MeshDiseaseTerms', NULL,NULL))
EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPane2' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'MeshDiseaseTerms'

GO

IF  EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_DiagramPane1' , N'SCHEMA',N'dbo', N'VIEW',N'MeshDiseaseTerms', NULL,NULL))
EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPane1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'MeshDiseaseTerms'

GO

/****** Object:  View [dbo].[MeshDiseaseTerms]    Script Date: 1/28/2014 11:23:32 AM ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[MeshDiseaseTerms]'))
DROP VIEW [dbo].[MeshDiseaseTerms]
GO

/****** Object:  View [dbo].[MeshDiseaseTerms]    Script Date: 1/28/2014 11:23:32 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[MeshDiseaseTerms]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[MeshDiseaseTerms]
AS
SELECT DISTINCT dn.String AS DescriptorName, c.ScopeNote, tn.TreeNumber
FROM            MeSH.dbo.Term AS t INNER JOIN
                         MeSH.dbo.TermList AS tl ON tl.TermList_Id = t.TermList_Id INNER JOIN
                         MeSH.dbo.Concept AS c ON c.Concept_Id = tl.Concept_Id INNER JOIN
                         MeSH.dbo.ConceptList AS cl ON cl.ConceptList_Id = c.ConceptList_Id INNER JOIN
                         MeSH.dbo.DescriptorRecord AS dr ON dr.DescriptorRecord_Id = cl.DescriptorRecord_Id INNER JOIN
                         MeSH.dbo.TreeNumberList AS tnl ON tnl.DescriptorRecord_Id = dr.DescriptorRecord_Id INNER JOIN
                         MeSH.dbo.TreeNumber AS tn ON tn.TreeNumberList_Id = tnl.TreeNumberList_Id INNER JOIN
                         MeSH.dbo.DescriptorName AS dn ON dn.DescriptorRecord_Id = dr.DescriptorRecord_Id AND t.String = dn.String
WHERE        (tn.TreeNumber LIKE ''C%'')
' 
GO

IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_DiagramPane1' , N'SCHEMA',N'dbo', N'VIEW',N'MeshDiseaseTerms', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "t"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 135
               Right = 264
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "tl"
            Begin Extent = 
               Top = 6
               Left = 302
               Bottom = 101
               Right = 472
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "c"
            Begin Extent = 
               Top = 6
               Left = 510
               Bottom = 135
               Right = 753
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "cl"
            Begin Extent = 
               Top = 6
               Left = 791
               Bottom = 101
               Right = 986
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "dr"
            Begin Extent = 
               Top = 6
               Left = 1024
               Bottom = 135
               Right = 1219
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "tnl"
            Begin Extent = 
               Top = 102
               Left = 302
               Bottom = 197
               Right = 497
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "tn"
            Begin Extent = 
               Top = 102
               Left = 791
               Bottom = 197
               Right = 980
            End
            DisplayFlags = 280
            TopColumn = 0
         E' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'MeshDiseaseTerms'
GO

IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_DiagramPane2' , N'SCHEMA',N'dbo', N'VIEW',N'MeshDiseaseTerms', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'nd
         Begin Table = "dn"
            Begin Extent = 
               Top = 138
               Left = 38
               Bottom = 250
               Right = 254
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'MeshDiseaseTerms'
GO

IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_DiagramPaneCount' , N'SCHEMA',N'dbo', N'VIEW',N'MeshDiseaseTerms', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'MeshDiseaseTerms'
GO


USE [DEV_RIT001]
GO

IF  EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_DiagramPaneCount' , N'SCHEMA',N'dbo', N'VIEW',N'MeshDiseaseSynonymTerms', NULL,NULL))
EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPaneCount' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'MeshDiseaseSynonymTerms'

GO

IF  EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_DiagramPane2' , N'SCHEMA',N'dbo', N'VIEW',N'MeshDiseaseSynonymTerms', NULL,NULL))
EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPane2' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'MeshDiseaseSynonymTerms'

GO

IF  EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_DiagramPane1' , N'SCHEMA',N'dbo', N'VIEW',N'MeshDiseaseSynonymTerms', NULL,NULL))
EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPane1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'MeshDiseaseSynonymTerms'

GO

/****** Object:  View [dbo].[MeshDiseaseSynonymTerms]    Script Date: 1/28/2014 11:24:32 AM ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[MeshDiseaseSynonymTerms]'))
DROP VIEW [dbo].[MeshDiseaseSynonymTerms]
GO

/****** Object:  View [dbo].[MeshDiseaseSynonymTerms]    Script Date: 1/28/2014 11:24:32 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[MeshDiseaseSynonymTerms]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[MeshDiseaseSynonymTerms]
AS
SELECT DISTINCT t.String AS Term, dn.String AS DescriptorName, tn.TreeNumber
FROM            MeSH.dbo.Term AS t INNER JOIN
                         MeSH.dbo.TermList AS tl ON tl.TermList_Id = t.TermList_Id INNER JOIN
                         MeSH.dbo.Concept AS c ON c.Concept_Id = tl.Concept_Id INNER JOIN
                         MeSH.dbo.ConceptList AS cl ON cl.ConceptList_Id = c.ConceptList_Id INNER JOIN
                         MeSH.dbo.DescriptorRecord AS dr ON dr.DescriptorRecord_Id = cl.DescriptorRecord_Id INNER JOIN
                         MeSH.dbo.TreeNumberList AS tnl ON tnl.DescriptorRecord_Id = dr.DescriptorRecord_Id INNER JOIN
                         MeSH.dbo.TreeNumber AS tn ON tn.TreeNumberList_Id = tnl.TreeNumberList_Id INNER JOIN
                         MeSH.dbo.DescriptorName AS dn ON dn.DescriptorRecord_Id = dr.DescriptorRecord_Id AND t.String <> dn.String
WHERE        (tn.TreeNumber LIKE ''C%'')
' 
GO

IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_DiagramPane1' , N'SCHEMA',N'dbo', N'VIEW',N'MeshDiseaseSynonymTerms', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "t"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 135
               Right = 264
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "tl"
            Begin Extent = 
               Top = 6
               Left = 302
               Bottom = 101
               Right = 472
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "c"
            Begin Extent = 
               Top = 6
               Left = 510
               Bottom = 135
               Right = 753
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "cl"
            Begin Extent = 
               Top = 6
               Left = 791
               Bottom = 101
               Right = 986
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "dr"
            Begin Extent = 
               Top = 6
               Left = 1024
               Bottom = 135
               Right = 1219
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "tnl"
            Begin Extent = 
               Top = 102
               Left = 302
               Bottom = 197
               Right = 497
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "tn"
            Begin Extent = 
               Top = 102
               Left = 791
               Bottom = 197
               Right = 980
            End
            DisplayFlags = 280
            TopColumn = 0
         E' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'MeshDiseaseSynonymTerms'
GO

IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_DiagramPane2' , N'SCHEMA',N'dbo', N'VIEW',N'MeshDiseaseSynonymTerms', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'nd
         Begin Table = "dn"
            Begin Extent = 
               Top = 138
               Left = 38
               Bottom = 250
               Right = 254
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'MeshDiseaseSynonymTerms'
GO

IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_DiagramPaneCount' , N'SCHEMA',N'dbo', N'VIEW',N'MeshDiseaseSynonymTerms', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'MeshDiseaseSynonymTerms'
GO


