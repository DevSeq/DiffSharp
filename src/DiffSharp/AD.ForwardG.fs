﻿//
// This file is part of
// DiffSharp -- F# Automatic Differentiation Library
//
// Copyright (C) 2014, National University of Ireland Maynooth.
//
//   DiffSharp is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   DiffSharp is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General Public License for more details.
//
//   You should have received a copy of the GNU General Public License
//   along with DiffSharp. If not, see <http://www.gnu.org/licenses/>.
//
// Written by:
//
//   Atilim Gunes Baydin
//   atilimgunes.baydin@nuim.ie
//
//   Barak A. Pearlmutter
//   barak@cs.nuim.ie
//
//   Hamilton Institute & Department of Computer Science
//   National University of Ireland Maynooth
//   Maynooth, Co. Kildare
//   Ireland
//
//   www.bcl.hamilton.ie
//

//
// Reference for "doublets": Dixon L., 2001, Automatic Differentiation: Calculation of the Hessian (http://dx.doi.org/10.1007/0-306-48332-7_17)
//

#light

/// Forward AD module, keeping vectors of gradient components
module DiffSharp.AD.ForwardG

open DiffSharp.Util.LinearAlgebra
open DiffSharp.Util.General

/// DualG numeric type, keeping a doublet of primal value and a vector of gradient components
// UNOPTIMIZED
type DualG =
    | DualG of float * Vector
    override d.ToString() = let (DualG(p, g)) = d in sprintf "DualG (%f, %A)" p g
    static member op_Explicit(p) = DualG(p, Vector.Zero)
    static member op_Explicit(DualG(p, _)) = p
    static member DivideByInt(DualG(p, g), i:int) = DualG(p / float i, g / float i)
    static member Zero = DualG(0., Vector.Zero)
    static member One = DualG(1., Vector.Zero)
    static member (+) (DualG(a, ag), DualG(b, bg)) = DualG(a + b, ag + bg)
    static member (-) (DualG(a, ag), DualG(b, bg)) = DualG(a - b, ag - bg)
    static member (*) (DualG(a, ag), DualG(b, bg)) = DualG(a * b, ag * b + a * bg)
    static member (/) (DualG(a, ag), DualG(b, bg)) = DualG(a / b, (ag * b - a * bg) / (b * b))
    static member Pow (DualG(a, ag), DualG(b, bg)) = DualG(a ** b, (a ** b) * ((b * ag / a) + ((log a) * bg)))
    static member (+) (DualG(a, ag), b) = DualG(a + b, ag)
    static member (-) (DualG(a, ag), b) = DualG(a - b, ag)
    static member (*) (DualG(a, ag), b) = DualG(a * b, ag * b)
    static member (/) (DualG(a, ag), b) = DualG(a / b, ag / b)
    static member Pow (DualG(a, ag), b) = DualG(a ** b, b * (a ** (b - 1.)) * ag)
    static member (+) (a, DualG(b, bg)) = DualG(b + a, bg)
    static member (-) (a, DualG(b, bg)) = DualG(a - b, -bg)
    static member (*) (a, DualG(b, bg)) = DualG(b * a, bg * a)
    static member (/) (a, DualG(b, bg)) = DualG(a / b, -a * bg / (b * b))
    static member Pow (a, DualG(b, bg)) = DualG(a ** b, (a ** b) * (log a) * bg)
    static member Log (DualG(a, ag)) = DualG(log a, ag / a)
    static member Exp (DualG(a, ag)) = DualG(exp a, ag * exp a)
    static member Sin (DualG(a, ag)) = DualG(sin a, ag * cos a)
    static member Cos (DualG(a, ag)) = DualG(cos a, -ag * sin a)
    static member Tan (DualG(a, ag)) = DualG(tan a, ag / ((cos a) * (cos a)))
    static member (~-) (DualG(a, ag)) = DualG(-a, -ag)
    static member Sqrt (DualG(a, ag)) = DualG(sqrt a, ag / (2. * sqrt a))
    static member Sinh (DualG(a, ag)) = DualG(sinh a, ag * cosh a)
    static member Cosh (DualG(a, ag)) = DualG(cosh a, ag * sinh a)
    static member Tanh (DualG(a, ag)) = DualG(tanh a, ag / ((cosh a) * (cosh a)))
    static member Asin (DualG(a, ag)) = DualG(asin a, ag / sqrt (1. - a * a))
    static member Acos (DualG(a, ag)) = DualG(acos a, -ag / sqrt (1. - a * a))
    static member agan (DualG(a, ag)) = DualG(atan a, ag / (1. + a * a))

/// DualG operations module (automatically opened)
[<AutoOpen>]
module DualGOps =
    /// Make DualG, with primal value `p`, gradient dimension `m`, and all gradient components 0
    let inline dualG p m = DualG(p, Vector.Create(m, 0.))
    /// Make DualG, with primal value `p` and gradient array `g`
    let inline dualGSet (p, g) = DualG(p, Vector.Create(g))
    /// Make active DualG (i.e. variable of differentiation), with primal value `p`, gradient dimension `m`, the component with index `i` having value 1, and the rest of the components 0
    let inline dualGAct p m i = DualG(p, Vector.Create(m, i, 1.))
    /// Make an array of active DualG, with primal values given in array `x`. For a DualG with index _i_, the gradient is the unit vector with 1 in the _i_th place.
    let inline dualGActArray (x:float[]) = Array.init x.Length (fun i -> dualGAct x.[i] x.Length i)
    /// Get the primal value of a DualG
    let inline primal (DualG(p, _)) = p
    /// Get the gradient array of a DualG
    let inline gradient (DualG(_, g)) = g.V
    /// Get the primal value and the first gradient component of a DualG, as a tuple
    let inline tuple (DualG(p, g)) = (p, g.V.[0])
    /// Get the primal value and the gradient array of a DualG, as a tuple
    let inline tupleG (DualG(p, g)) = (p, g.V)


/// ForwardG differentiation operations module (automatically opened)
[<AutoOpen>]
module ForwardGOps =
    /// Original value and first derivative of a scalar-to-scalar function `f`
    let inline diff' f =
        fun x -> dualGAct x 1 0 |> f |> tuple

    /// First derivative of a scalar-to-scalar function `f`
    let inline diff f =
        diff' f >> snd

    /// Original value and gradient of a vector-to-scalar function `f`
    let inline grad' f =
        dualGActArray >> f >> tupleG

    /// Gradient of a vector-to-scalar function `f`
    let inline grad f =
        grad' f >> snd
    
    /// Original value and Jacobian of a vector-to-vector function `f`
    let inline jacobian' f =
        fun x ->
            let a = dualGActArray x |> f
            (Array.map primal a, Matrix.Create(a.Length, fun i -> gradient a.[i]).M)

    /// Jacobian of a vector-to-vector function `f`
    let inline jacobian f =
        jacobian' f >> snd

    /// Original value and transposed Jacobian of a vector-to-vector function `f`
    let inline jacobianT' f =
        fun x -> let (v, j) = jacobian' f x in (v, transpose j)

    /// Transposed Jacobian of a vector-to-vector function `f`
    let inline jacobianT f =
        jacobianT' f >> snd


/// Module with differentiation operators using Vector and Matrix input and output, instead of float[] and float[,]
module Vector =
    /// Original value and first derivative of a scalar-to-scalar function `f`
    let inline diff' f = ForwardGOps.diff' f
    /// First derivative of a scalar-to-scalar function `f`
    let inline diff f = ForwardGOps.diff f
    /// Original value and gradient of a vector-to-scalar function `f`
    let inline grad' f = array >> ForwardGOps.grad' f >> fun (a, b) -> (a, vector b)
    /// Gradient of a vector-to-scalar function `f`
    let inline grad f = array >> ForwardGOps.grad f >> vector
    /// Original value and transposed Jacobian of a vector-to-vector function `f`
    let inline jacobianT' f = array >> ForwardGOps.jacobianT' f >> fun (a, b) -> (vector a, matrix b)
    /// Transposed Jacobian of a vector-to-vector function `f`
    let inline jacobianT f = array >> ForwardGOps.jacobianT f >> matrix
    /// Original value and Jacobian of a vector-to-vector function `f`
    let inline jacobian' f = array >> ForwardGOps.jacobian' f >> fun (a, b) -> (vector a, matrix b)
    /// Jacobian of a vector-to-vector function `f`
    let inline jacobian f = array >> ForwardGOps.jacobian f >> matrix
