using System.Collections.Generic;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Processors.Components;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Entities
{
    [Processor("MeshInstanceEntity")]
    public class MeshInstanceEntityProcessor : EntityProcessor
    {
        MeshFilterProcessor _meshFilterProcessor = new MeshFilterProcessor();
        MeshRendererProcessor _meshRendererProcessor = new MeshRendererProcessor();

        public override IEnumerable<GameObject> Process(IGenerationContext context, JToken entityToken)
        {
            GameObject gameObject = new GameObject("Mesh Instance Entity");

            //fill in the name, static, enabled, tag and layer fields
            ProcessCommonUnityAttributes(context, gameObject, entityToken);

            gameObject.transform.position = entityToken["Position"].ToVector3();
            gameObject.transform.rotation *= Quaternion.LookRotation(entityToken["ForwardVector"].ToVector3(), entityToken["UpVector"].ToVector3());
            gameObject.transform.localScale = entityToken["Scale"].ToVector3();

            //use the processors already defined for the component
            _meshFilterProcessor.Process(context, gameObject, entityToken["MeshFilter"]);
            _meshRendererProcessor.Process(context, gameObject, entityToken["MeshRenderer"]);

            yield return gameObject;
        }
    }
}