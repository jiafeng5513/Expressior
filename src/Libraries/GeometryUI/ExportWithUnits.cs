using System;
using System.Collections.Generic;

using Autodesk.DesignScript.Geometry;

using Dynamo.Models;
using Dynamo.Utilities;
using DynamoConversions;

using GeometryUI.Properties;

using ProtoCore.AST.AssociativeAST;
using System.Xml;
using System.Globalization;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
/*
 * 带客制化界面的元素的节点API
 * 1.输出到bin/node中.
 * 2.不需要在PathResolvers.cs中加载DLL,node目录中的dll是由NodeModelAssemblyLoader.cs加载的
 * 3.与zerotouchlibrary的编写有较大的区别,不是通过函数的访问控制符来控制Node是否出现在list中的.
 * 4.属性说明:
 *       [NodeName("ExportToSAT")]Node的名字
 */
namespace GeometryUI
{
    [NodeCategory(BuiltinNodeCategories.GEOMETRY)]
    [NodeName("MUL")]
    [InPortTypes("string")]
    [NodeDescription("ExportToSATDescripiton", typeof(GeometryUI.Properties.Resources))]
    [NodeSearchTags("ExportWithUnitsSearchTags", typeof(GeometryUI.Properties.Resources))]
    [IsDesignScriptCompatible]
    public class ExportWithUnits : NodeModel
    {
        private int valueofslider = 50;


        public int ValueofsliderOfSlider
        {
            get { return valueofslider; }
            set
            {
                valueofslider = value;
                this.OnNodeModified();
                RaisePropertyChanged("ValueofsliderOfSlider");
            }
        }



        [JsonConstructor]
        private ExportWithUnits(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts):base(inPorts, outPorts)
        {
            ShouldDisplayPreviewCore = true;
        }

        public ExportWithUnits()
        {


            InPorts.Add(new PortModel(PortType.Input, this, new PortData("output1", Resources.ExportToSatGeometryInputDescription)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("filePath", Resources.ExportToSatFilePathDescription, new StringNode())));
            //OutPorts.Add(new PortModel(PortType.Output, this, new PortData("string", Resources.ExportToSatFilePathOutputDescription)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("Test", "看到就是成功")));

            ShouldDisplayPreviewCore = true;
            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (!InPorts[0].IsConnected || !InPorts[1].IsConnected)
            {
                var rhs = AstFactory.BuildIntNode(ValueofsliderOfSlider);//这里是突破口,可以注入外部类型
                var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);

                return new[] { assignment };
                
                //return new[] {AstFactory.BuildAssignment(new ArgumentSignatureNode(), new ArgumentSignatureNode())};
                //return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }

            //double unitsMM = Conversions.ConversionDictionary[SelectedExportedUnit]*1000.0;

            var geometryListNode = inputAstNodes[0];
            var filePathNode = inputAstNodes[1];
            ;
            AssociativeNode node = AstFactory.BuildStringNode(
                "In1:" + ((StringNode)geometryListNode).Value+ "\n" +
                "In2:" + filePathNode + "\n" +
                "Slider:" + ValueofsliderOfSlider);
            AstExtensions.ToImperativeAST(geometryListNode);


            //node = AstFactory.BuildFunctionCall(
            //            new Func<IEnumerable<Geometry>, string, double, string>(Geometry.ExportToSAT),
            //            new List<AssociativeNode> { geometryListNode, filePathNode, unitsMMNode });

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node)};
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); // Base implementation must be called.

            var helper = new XmlElementHelper(element);
            helper.SetAttribute("UINodeExample", valueofslider.ToString());//把slider的值存起来
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called.
            var helper = new XmlElementHelper(element);
            var exportedUnit = helper.ReadString("UINodeExample");

            valueofslider = int.Parse(exportedUnit) is int ? int.Parse(exportedUnit) : 0;
        }

        #endregion
    }
}

