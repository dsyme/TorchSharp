// Copyright (c) Microsoft Corporation and contributors.  All Rights Reserved.  See License.txt in the project root for license information.
using System;
using System.Runtime.InteropServices;
using TorchSharp.Tensor;

namespace TorchSharp.NN
{
    public enum EmbeddingBagMode
    {
        Sum = 0,
        Mean = 1,
        Max = 2
    }

    public class EmbeddingBag : Module
    {
        internal EmbeddingBag (IntPtr handle, IntPtr boxedHandle) : base (handle, boxedHandle) { }

        [DllImport ("LibTorchSharp")]
        private static extern IntPtr THSNN_EmbeddingBag_forward (Module.HType module, IntPtr tensor, IntPtr offsets, IntPtr per_sample_weights);

        public TorchTensor forward (TorchTensor input, TorchTensor offsets = null, TorchTensor perSampleWeights = null)
        {
            if (!input.IsIntegral()) throw new ArgumentException("Embedding input must be an integral tensor.");
            if (!(offsets is null) && input.Type != offsets.Type) throw new ArgumentException("input and offsets must have the same element type.");
            if (input.Dimensions == 1 && offsets is null) throw new ArgumentException("'offsets' must be non-null for a 1-D input.");
            if (input.Dimensions == 2 && !(offsets is null)) throw new ArgumentException("'offsets' must be null for a 2-D input.");

            if (input.Dimensions == 2 && input.Type == ScalarType.Int32) throw new NotImplementedException("EmbeddingBag for 32-bit integers -- there's some issue in the native runtime that prevents this from working.");

            var res = THSNN_EmbeddingBag_forward(handle, input.Handle, (offsets is null) ? IntPtr.Zero : offsets.Handle, (perSampleWeights is null) ? IntPtr.Zero : perSampleWeights.Handle);
            if (res == IntPtr.Zero) { Torch.CheckForErrors(); }
            return new TorchTensor (res);
        }

        [DllImport("LibTorchSharp")]
        extern static IntPtr THSNN_EmbeddingBag_weight(Module.HType module);

        [DllImport("LibTorchSharp")]
        extern static void THSNN_EmbeddingBag_set_weight(Module.HType module, IntPtr tensor);

        public TorchTensor Weight {
            get {
                var res = THSNN_EmbeddingBag_weight(handle);
                if (res == IntPtr.Zero) { Torch.CheckForErrors(); }
                return new TorchTensor(res);
            }
            set {
                THSNN_EmbeddingBag_set_weight(handle, value.Handle);
                Torch.CheckForErrors();
            }
        }

        [DllImport("LibTorchSharp")]
        private static extern IntPtr THSNN_EmbeddingBag_from_pretrained(IntPtr embeddings, bool freeze, double max_norm, bool hasMN, double norm_type, bool scale_grad_by_freq, long mode, bool sparse, bool include_last_offset, out IntPtr pBoxedModule);

        /// <summary>
        /// A simple lookup table that stores embeddings of a fixed dictionary and size.
        /// This module is often used to store word embeddings and retrieve them using indices. The input to the module is a list of indices, and the output is the corresponding word embeddings.
        /// </summary>
        /// <param name="embeddings">FloatTensor containing weights for the EmbeddingBag in two dimensions. First dimension is being passed to EmbeddingBag as num_embeddings, second as embedding_dim.</param>
        /// <param name="freeze">If true (the default), the tensor does not get updated in the learning</param>
        /// <param name="max_norm">If given, each embedding vector with norm larger than max_norm is renormalized to have norm max_norm.</param>
        /// <param name="norm_type">The p of the p-norm to compute for the max_norm option. Default 2.</param>
        /// <param name="scale_grad_by_freq">If given, this will scale gradients by the inverse of frequency of the words in the mini-batch. Default: false.</param>
        /// <param name="mode"></param>
        /// <param name="sparse">If true, gradient w.r.t. weight matrix will be a sparse tensor. Default: false</param>
        /// <param name="include_last_offset"></param>
        /// <returns></returns>
        /// <remarks>Keep in mind that only a limited number of optimizers support sparse gradients: currently it’s optim.SGD (CUDA and CPU), optim.SparseAdam (CUDA and CPU) and optim.Adagrad (CPU)</remarks>
        public static EmbeddingBag from_pretrained(TorchTensor embeddings, bool freeze = true, double? max_norm = null, double norm_type = 2.0, bool scale_grad_by_freq = false, EmbeddingBagMode mode = EmbeddingBagMode.Mean, bool sparse = false, bool include_last_offset = false)
        {
            var res = THSNN_EmbeddingBag_from_pretrained(embeddings.Handle, freeze,
                max_norm.HasValue ? max_norm.Value : 0.0, max_norm.HasValue,
                norm_type, scale_grad_by_freq, (long)mode, sparse, include_last_offset, out var boxedHandle);
            if (res == IntPtr.Zero) { Torch.CheckForErrors(); }
            return new EmbeddingBag(res, boxedHandle);

        }

    }
    public static partial class Modules
    {
        [DllImport ("LibTorchSharp")]
        private static extern IntPtr THSNN_EmbeddingBag_ctor (long num_embeddings, long embedding_dims, double max_norm, bool hasMN, double norm_type, bool scale_grad_by_freq, long mode, bool sparse, bool include_last_offset, out IntPtr pBoxedModule);

        /// <summary>
        /// A simple lookup table that stores embeddings of a fixed dictionary and size.
        /// This module is often used to store word embeddings and retrieve them using indices. The input to the module is a list of indices, and the output is the corresponding word embeddings.
        /// </summary>
        /// <param name="num_embeddings">Size of the dictionary of embeddings, the vocabulary size.</param>
        /// <param name="embedding_dims">The size of each embedding vector</param>
        /// <param name="max_norm">If given, each embedding vector with norm larger than max_norm is renormalized to have norm max_norm.</param>
        /// <param name="norm_type">The p of the p-norm to compute for the max_norm option. Default 2.</param>
        /// <param name="scale_grad_by_freq">If given, this will scale gradients by the inverse of frequency of the words in the mini-batch. Default: false.</param>
        /// <param name="mode">"sum", "mean" or "max". Specifies the way to reduce the bag.
        /// "sum" computes the weighted sum, taking per_sample_weights into consideration.
        /// "mean" computes the average of the values in the bag, "max" computes the max value over each bag. Default: "mean"</param>
        /// <param name="sparse">If true, gradient w.r.t. weight matrix will be a sparse tensor. Default: false</param>
        /// <param name="include_last_offset">if true, offsets has one additional element, where the last element is equivalent to the size of indices.</param>
        /// <returns></returns>
        /// <remarks>Keep in mind that only a limited number of optimizers support sparse gradients: currently it’s optim.SGD (CUDA and CPU), optim.SparseAdam (CUDA and CPU) and optim.Adagrad (CPU)</remarks>
        static public EmbeddingBag EmbeddingBag (long num_embeddings, long embedding_dims, double? max_norm = null, double norm_type = 2.0, bool scale_grad_by_freq = false, EmbeddingBagMode mode = EmbeddingBagMode.Mean, bool sparse = false, bool include_last_offset = false)
        {
            var res = THSNN_EmbeddingBag_ctor (num_embeddings, embedding_dims,
                max_norm.HasValue ? max_norm.Value : 0.0, max_norm.HasValue,
                norm_type, scale_grad_by_freq, (long)mode, sparse, include_last_offset, out var boxedHandle);
            if (res == IntPtr.Zero) { Torch.CheckForErrors(); }
            return new EmbeddingBag (res, boxedHandle);
        }
    }
    public static partial class Functions
    {
        /// <summary>
        /// A simple lookup table that stores embeddings of a fixed dictionary and size.
        /// This module is often used to store word embeddings and retrieve them using indices. The input to the module is a list of indices, and the output is the corresponding word embeddings.
        /// </summary>
        /// <param name="x">An input tensor of arbitrary shape.</param>
        /// <param name="num_embeddings">Size of the dictionary of embeddings, the vocabulary size.</param>
        /// <param name="embedding_dims">The size of each embedding vector</param>
        /// <param name="max_norm">If given, each embedding vector with norm larger than max_norm is renormalized to have norm max_norm.</param>
        /// <param name="norm_type">The p of the p-norm to compute for the max_norm option. Default 2.</param>
        /// <param name="scale_grad_by_freq">If given, this will scale gradients by the inverse of frequency of the words in the mini-batch. Default: false.</param>
        /// <param name="mode">"sum", "mean" or "max". Specifies the way to reduce the bag.
        /// "sum" computes the weighted sum, taking per_sample_weights into consideration.
        /// "mean" computes the average of the values in the bag, "max" computes the max value over each bag. Default: "mean"</param>
        /// <param name="sparse">If true, gradient w.r.t. weight matrix will be a sparse tensor. Default: false</param>
        /// <param name="include_last_offset">if true, offsets has one additional element, where the last element is equivalent to the size of indices.</param>
        /// <returns></returns>
        /// <remarks>Keep in mind that only a limited number of optimizers support sparse gradients: currently it’s optim.SGD (CUDA and CPU), optim.SparseAdam (CUDA and CPU) and optim.Adagrad (CPU)</remarks>
        static public TorchTensor EmbeddingBag (TorchTensor x, long num_embeddings, long embedding_dims, double? max_norm = null, double norm_type = 2.0, bool scale_grad_by_freq = false, EmbeddingBagMode mode = EmbeddingBagMode.Mean, bool sparse = false, bool include_last_offset = false)
        {
            using (var d = Modules.EmbeddingBag(num_embeddings, embedding_dims, max_norm, norm_type, scale_grad_by_freq, mode, sparse, include_last_offset)) {
                return d.forward (x);
            }
        }
    }

}
