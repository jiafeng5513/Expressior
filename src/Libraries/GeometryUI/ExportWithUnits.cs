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
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeName("UINodeExample")]
    [NodeDescription("UINodeExampleDescripiton", typeof(GeometryUI.Properties.Resources))]
    [NodeSearchTags("UINodeExampleSearchTags", typeof(GeometryUI.Properties.Resources))]
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
            //TODO:内部变量的初始化
            ShouldDisplayPreviewCore = true;
        }

        public ExportWithUnits()
        {
            //TODO:内部变量初始化

            //构造输入输出端口
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("InputPort1", Resources.InputPort1Description)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("InputPort2", Resources.InputPort2Description, new StringNode())));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("OutputPort1", Resources.OutputPort1Description)));

            ShouldDisplayPreviewCore = true;
            RegisterAllPorts();
        }
        /// <summary>
        /// 构建输出节点
        /// 这个函数的触发条件是输入节点发生了变化,即this.OnNodeModified();
        /// </summary>
        /// <param name="inputAstNodes"></param>
        /// <returns></returns>
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (!InPorts[0].IsConnected || !InPorts[1].IsConnected)
            {
                //如果在没有连接输入节点的情况下连接了输出节点,应当有默认输出
                var rhs = AstFactory.BuildIntNode(valueofslider);//TODO:AstFactory.BuildIntNode,向其中注入类型:Mat
                var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);
                return new[] { assignment };
            }

            //取输入端口值
            var geometryListNode = inputAstNodes[0];
            var filePathNode = inputAstNodes[1];
            AssociativeNode node = AstFactory.BuildStringNode(
                "In1:" + ((StringNode)geometryListNode).Value+"\n"+
                "In2:" + ((StringNode)filePathNode).Value+"\n"+
                "Slider:"+ValueofsliderOfSlider);

            //node = AstFactory.BuildFunctionCall(
            //            new Func<IEnumerable<Geometry>, string, double, string>(Geometry.ExportToSAT),
            //            new List<AssociativeNode> { geometryListNode, filePathNode, unitsMMNode });

            //注意观察多输出怎么写
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node)};
        }

        #region 重载:序列化和解序列化方法
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

            valueofslider =  int.Parse(exportedUnit) is int ? int.Parse(exportedUnit) : 0; 
        }

        #endregion
    }
}

