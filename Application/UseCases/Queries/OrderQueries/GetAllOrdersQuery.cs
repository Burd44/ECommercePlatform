﻿using Application.DTOs;
using Domain.Common;
using MediatR;

namespace Application.UseCases.Queries
{
    public class GetAllOrdersQuery : IRequest<Result<IEnumerable<OrderDto>>>
    {
    }
}